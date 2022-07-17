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

    private readonly PhysicsFunctions _functions = new PhysicsFunctions();
    private const float _jumpScale = 12f;
    private const float _moveScale = 8f;
    private const float _baseMaxHorizontalSpeed = 20f;
    private const float _totalMaxHorizontalSpeed = 40f;
    private const float _maxVerticalSpeed = 40f;
    private const float _stickySpeed = _totalMaxHorizontalSpeed / 4f;
    private const float _groundOffset = .01f;
    private const float _maxNextNormalAngle = 50f;
    private const float _accelerationTime = .0005f;
    private float _groundFriction;
    private float _airFriction;
    private float _groundHitDistance;
    private bool _grounded;
    private bool _lastGrounded;
    private Rigidbody2D _rigidbody = null!;
    private CircleCollider2D _bodyCollider = null!;
    private BoxCollider2D _leftLeg = null!;
    private BoxCollider2D _rightLeg = null!;
    private ContactFilter2D _contactFilter;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Vector2 _velocity;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    public void SetVelocity(Vector2 velocity) =>
        _velocity = velocity.RotatedBy(Vector2.SignedAngle(_groundNormal, Vector2.up));

    public void SetLayer(int layer)
    {
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(layer));
        gameObject.layer = layer;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<CircleCollider2D>();
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

        const float circleBoxCollidersCastDifference = .01f;
        var size = _bodyCollider.bounds.size;
        var boxSize = new Vector2(size.x * .49f, size.x / 2 - circleBoxCollidersCastDifference);
        _leftLeg.size = boxSize;
        _rightLeg.size = boxSize;

        var offset = -size / 4;
        offset.y += circleBoxCollidersCastDifference / 2;
        _leftLeg.offset = offset;
        _rightLeg.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        var gravity = _groundNormal * (Physics2D.gravity.y * Time.deltaTime);
        float friction = _velocity.x.Sign() * (_grounded ? _groundFriction : _airFriction);
        float speed = Math.Abs(_velocity.x);

        _velocity.x += _targetVelocity.x * _moveScale * (_accelerationTime / Time.deltaTime) - gravity.x;
        _velocity.x = MyMath.ClampBetween(_velocity.x - friction, _velocity.x, 0f);
        float totalSpeed = Math.Abs(_velocity.x);

        if (totalSpeed > _baseMaxHorizontalSpeed && totalSpeed > speed)
        {
            _velocity.x = speed * _velocity.x.Sign();
            var along = CalculateMoveAlong(_groundNormal) * _velocity.x.Sign();

            if (along.y < 0)
                _velocity.x += (totalSpeed - speed) * -along.y * _velocity.x.Sign();
        }

        _velocity.y += gravity.y;

        if (_grounded && _targetVelocity.y > 0)
            _velocity.y += _targetVelocity.y * _jumpScale;

        _velocity.x = Math.Clamp(_velocity.x, -_totalMaxHorizontalSpeed, _totalMaxHorizontalSpeed);
        _velocity.y = Math.Clamp(_velocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _targetVelocity = Vector2.zero;
        _rigidbody.velocity = Vector2.zero;

        var deltaPosition = _velocity * Time.deltaTime;

        if (_grounded && _velocity.y < 0)
            UpdateGroundNormal(-deltaPosition.magnitude - _groundOffset);
        else
            UpdateGroundNormal(deltaPosition.y + deltaPosition.y.Sign() * _groundOffset);

        ChangeBodyVelocityByGravity();

        var move = CalculateMoveAlong(_groundNormal) * (_velocity * Time.deltaTime).x;
        ChangeBodyVelocity(move);

        _setHorizontalForce.Invoke(_velocity.x / _totalMaxHorizontalSpeed);
        _setGrounded.Invoke(_grounded);

        _rigidbody.SetRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private bool IsValidNextGroundNormal(Vector2 newNormal, float maxAngleDifference = _maxNextNormalAngle) =>
        newNormal != Vector2.zero && Vector2.Angle(_groundNormal, newNormal) < maxAngleDifference;

    private void ChangeBodyVelocityByGravity()
    {
        _groundHitDistance -= _groundOffset;

        if (!_grounded && _groundHitDistance < 0)
            _groundHitDistance = 0;

        int dir = _velocity.y.Sign() <= 0 ? -1 : 1;
        _groundHitDistance *= dir;
        _rigidbody.velocity += _groundNormal * _groundHitDistance / Time.deltaTime;

        _groundHitDistance = _groundOffset;
    }

    private void ChangeBodyVelocity(Vector2 move, float maxRecursion = 3)
    {
        if (move == Vector2.zero)
            return;

        float deltaMagnitude = move.magnitude;

        (var normal, float distance, int layer) =
            _functions.FindNearestNormal(move, deltaMagnitude,
                _bodyCollider, _contactFilter);

        float distanceWithGroundOffset = distance - _groundOffset;

        if (normal == Vector2.zero)
            _rigidbody.velocity += move / Time.deltaTime;
        else
            _rigidbody.velocity += move * (distanceWithGroundOffset / deltaMagnitude) / Time.deltaTime;

        if (maxRecursion <= 0 || normal == Vector2.zero || _slopeNormal != _groundNormal)
            return;

        if (!IsValidNextGroundNormal(normal))
            ChangeVelocityByNormal(normal);
        else
            ChangeBodyVelocity(
                CalculateMoveAlong(normal) * ((deltaMagnitude - distanceWithGroundOffset) * _velocity.x.Sign()),
                --maxRecursion);
    }

    private void UpdateGroundNormal(float verticalForce)
    {
        int dir = verticalForce.Sign() == 0 ? -1 : verticalForce.Sign();
        float castDistance = Math.Abs(verticalForce);

        (var leftNormal, float leftDistance, int leftLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _leftLeg, _contactFilter);

        (var rightNormal, float rightDistance, int rightLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _rightLeg, _contactFilter);

        (var normal, float distance, int layer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _bodyCollider, _contactFilter);

        bool centerNormalIsValid = normal != Vector2.zero;
        var leftNormalIsValid = leftDistance > 0 &&
                                IsValidNextGroundNormal(leftNormal,
                                    centerNormalIsValid && leftDistance > distance ? _maxNextNormalAngle : 1);
        var rightNormalIsValid = rightDistance > 0 &&
                                 IsValidNextGroundNormal(rightNormal,
                                     centerNormalIsValid && rightDistance > distance ? _maxNextNormalAngle : 1);

        if (leftNormalIsValid && !rightNormalIsValid)
            UpdateGroundNormal(force: verticalForce, normal: leftNormal, distance: leftDistance, layer: leftLayer);
        else if (rightNormalIsValid && !leftNormalIsValid)
            UpdateGroundNormal(force: verticalForce, normal: rightNormal, distance: rightDistance, layer: rightLayer);
        else
            UpdateGroundNormal(force: verticalForce, normal: normal, distance: distance, layer: layer);
    }

    private void UpdateGroundNormal(Vector2 normal, float distance, float force, int layer)
    {
        _lastGrounded = _grounded;
        _grounded = false;
        _groundHitDistance = distance;

        if (normal == Vector2.zero)
        {
            if (_lastGrounded)
            {
                _groundHitDistance = 0;
                _velocity = _velocity.RotatedBy(Vector2.SignedAngle(Vector2.up, _slopeNormal));
            }

            _groundNormal = Vector2.up;
            _slopeNormal = Vector2.up;
            return;
        }

        if (IsValidNextGroundNormal(normal))
        {
            _groundNormal = normal;
            _slopeNormal = _groundNormal;
            _grounded = true;

            if (!_lastGrounded)
                ChangeVelocityByNormal(_groundNormal);

            SetLayer(layer);
            _velocity.y = Physics2D.gravity.y * Time.deltaTime;
            _velocity.y *= Mathf.Abs(_velocity.x) > _stickySpeed ? 2 : .5f;
            // _velocity.y = -2;
            return;
        }

        _groundHitDistance = Math.Abs(force);
        _slopeNormal = normal;
        ChangeVelocityByNormal(normal);
    }

    private void ChangeVelocityByNormal(Vector2 normal) => _velocity = (_velocity + _velocity.ReflectBy(normal)) / 2;
}