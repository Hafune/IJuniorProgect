using System;
using Lib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PhysicsSonic2D : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _setHorizontalForce;
    [SerializeField] private UnityEvent<bool> _setGrounded;

    private Rigidbody2D _rigidbody = null!;
    private Collider2D _bodyCollider = null!;
    private BoxCollider2D _leftLeg = null!;
    private BoxCollider2D _rightLeg = null!;
    private ContactFilter2D _contactFilter;
    private readonly PhysicsFunctions _functions = new PhysicsFunctions();
    private const float _jumpScale = 10f;
    private const float _moveScale = 8f;
    private const float _maxHorizontalSpeed = 40f;
    private const float _maxVerticalSpeed = 40f;
    private const float _stickySpeed = _maxHorizontalSpeed / 2f;
    private float _groundOffset = .01f;
    private const float _maxNormalAngleDifference = 50f;
    private const float _accelerationTime = .001f;
    private float _groundFriction;
    private float _airFriction;
    private float _groundHitDistance;
    private bool _grounded;
    private bool _lastGrounded;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Vector2 _velocity;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<Collider2D>();
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;
        _groundNormal = Quaternion.Euler(0f, 0f, _rigidbody.rotation) * _groundNormal;

        _leftLeg = transform.AddComponent<BoxCollider2D>();
        _rightLeg = transform.AddComponent<BoxCollider2D>();

        _leftLeg.isTrigger = true;
        _rightLeg.isTrigger = true;

        _groundFriction = _accelerationTime / Time.fixedDeltaTime * 2;
        _airFriction = _accelerationTime / Time.fixedDeltaTime * .01f;

        var size = _bodyCollider.bounds.size;
        var boxSize = new Vector2(size.x / 2, size.x / 2);
        _leftLeg.size = boxSize;
        _rightLeg.size = boxSize;
        Application.targetFrameRate = 10000;
        var offset = -boxSize / 2;
        _leftLeg.offset = offset;
        _rightLeg.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        var gravity = _groundNormal * (Physics2D.gravity.y * Time.deltaTime);
        var friction = Math.Sign(_velocity.x) * (_grounded ? _groundFriction : _airFriction);

        _velocity.x += _targetVelocity.x * _moveScale * (_accelerationTime / Time.deltaTime) - gravity.x;
        _velocity.x = MyMath.ClampBetween(_velocity.x - friction, _velocity.x, 0f);
        _velocity.y += gravity.y;

        if (_grounded && _targetVelocity.y > 0)
            _velocity.y += _targetVelocity.y * _jumpScale;

        _velocity.x = Math.Clamp(_velocity.x, -_maxHorizontalSpeed, _maxHorizontalSpeed);
        _velocity.y = Math.Clamp(_velocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _targetVelocity = Vector2.zero;
        _rigidbody.velocity = Vector2.zero;

        var deltaPosition = _velocity * Time.deltaTime;

        if (_grounded && _velocity.y < 0)
            UpdateGroundNormal(-deltaPosition.magnitude - _groundOffset);
        else
            UpdateGroundNormal(deltaPosition.y + Mathf.Sign(deltaPosition.y) * _groundOffset);

        ChangePositionByGravity();

        var move = CalculateMoveAlong(_groundNormal) * deltaPosition.x;
        ChangePosition(move);

        _setHorizontalForce.Invoke(_velocity.x / _maxHorizontalSpeed);
        _setGrounded.Invoke(_grounded);

        _rigidbody.SetRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private bool validateNextGroundNormal(Vector2 newNormal, float maxAngleDifference = _maxNormalAngleDifference) =>
        newNormal != Vector2.zero && Vector2.Angle(_groundNormal, newNormal) < maxAngleDifference;

    private void ChangePositionByGravity()
    {
        _groundHitDistance -= _groundOffset;

        if (!_grounded && _groundHitDistance < 0)
            _groundHitDistance = 0;

        int dir = Math.Sign(_velocity.y) <= 0 ? -1 : 1;
        _groundHitDistance *= dir;
        _rigidbody.velocity += _groundNormal * _groundHitDistance / Time.deltaTime;
    }

    private void ChangePosition(Vector2 move, float maxRecursion = 1)
    {
        if (move == Vector2.zero)
            return;

        float deltaMagnitude = move.magnitude;

        (var normal, float distance, int layer) =
            _functions.FindNearestNormal(move, deltaMagnitude,
                _bodyCollider, _contactFilter);

        float tail = deltaMagnitude - distance;

        if (normal == Vector2.zero)
            _rigidbody.velocity += move / Time.deltaTime;
        else
            _rigidbody.velocity += move * (tail / deltaMagnitude) / Time.deltaTime;

        if (maxRecursion <= 0 || normal == Vector2.zero || _slopeNormal != _groundNormal)
            return;

        if (!validateNextGroundNormal(normal))
            _velocity.x = 0f;
        else
            ChangePosition(CalculateMoveAlong(normal) * ((deltaMagnitude - tail) * Math.Sign(_velocity.x)),
                --maxRecursion);
    }

    private void UpdateGroundNormal(float verticalForce)
    {
        _lastGrounded = _grounded;
        _grounded = false;
        _leftLeg.isTrigger = true;
        _rightLeg.isTrigger = true;

        int dir = Math.Sign(verticalForce) == 0 ? -1 : Math.Sign(verticalForce);
        float castDistance = Math.Abs(verticalForce);

        var (leftNormal, lDistance, leftLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _leftLeg, _contactFilter);

        var (rightNormal, rDistance, rightLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _rightLeg, _contactFilter);

        var (normal, distance, layer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _bodyCollider, _contactFilter);

        var leftNormalIsValid = validateNextGroundNormal(leftNormal, 10);
        var rightNormalIsValid = validateNextGroundNormal(rightNormal, 10);

        if (leftNormalIsValid && !rightNormalIsValid)
        {
            _leftLeg.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: leftNormal, distance: lDistance, layer: leftLayer);
        }
        else if (rightNormalIsValid && !leftNormalIsValid)
        {
            _rightLeg.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: rightNormal, distance: rDistance, layer: rightLayer);
        }
        else
        {
            UpdateGroundNormal(force: verticalForce, normal: normal, distance: distance, layer: layer);

            if (_grounded || normal == Vector2.zero)
                return;

            ChangeVelocityByNormal(normal);
        }
    }

    private void UpdateGroundNormal(Vector2 normal, float distance, float force, int layer)
    {
        _groundHitDistance = distance;

        if (normal == Vector2.zero)
        {
            if (!_grounded && _lastGrounded)
                _velocity = _velocity.RotateBy(Vector2.SignedAngle(Vector2.up, _slopeNormal));

            _groundNormal = Vector2.up;
            _slopeNormal = Vector2.up;
            return;
        }

        if (validateNextGroundNormal(normal))
        {
            _groundNormal = normal;
            _slopeNormal = _groundNormal;
            _grounded = true;

            if (!_lastGrounded)
                ChangeVelocityByNormal(_groundNormal);

            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(layer));
            gameObject.layer = layer;
            _velocity.y = Physics2D.gravity.y * Time.deltaTime;
            _velocity.y *= Mathf.Abs(_velocity.x) > _stickySpeed ? 2 : 1;
            // _velocity.y = -2;
            return;
        }

        _groundHitDistance = Math.Abs(force);
        _slopeNormal = normal;
        // ChangeVelocityByNormal(normal.y < 0 ? -normal : normal);
    }

    private void ChangeVelocityByNormal(Vector2 normal) => _velocity = (_velocity + _velocity.ReflectBy(normal)) / 2;
}