using System;
using Lib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PhysicsSonic2D : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
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
    private const float _maxHorizontalSpeed = 8f;
    private const float _maxVerticalSpeed = 40f;
    private const float _groundOffset = .004f;
    private const float _maxNormalAngle = 50f;
    private const float _accelerationTime = .001f;
    private float _groundFriction;
    private float _airFriction;
    private float _hitDistance;
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
        _contactFilter.SetLayerMask(_layerMask);
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

        var offset = -boxSize / 2;
        _leftLeg.offset = offset;
        _rightLeg.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        _velocity.x += _targetVelocity.x * _moveScale * (_accelerationTime / Time.deltaTime);
        _velocity.x = MyFunc.ClampBetween(_velocity.x - Math.Sign(_velocity.x) *
            (_grounded ? _groundFriction : _airFriction), _velocity.x, 0f);
        _velocity.y += Physics2D.gravity.y * Time.deltaTime;

        if (_grounded && _targetVelocity.y > 0)
            _velocity.y += _targetVelocity.y * _jumpScale;

        _targetVelocity = Vector2.zero;

        _velocity.x = Math.Clamp(_velocity.x, -_maxHorizontalSpeed, _maxHorizontalSpeed);
        _velocity.y = Math.Clamp(_velocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);

        var deltaPosition = _velocity * Time.deltaTime;
        var move = CalculateMoveAlong(_groundNormal) * deltaPosition.x;

        _rigidbody.velocity = Vector2.zero;

        if (_grounded && _velocity.y < 0)
            UpdateGroundNormal(-deltaPosition.magnitude);
        else
            UpdateGroundNormal(deltaPosition.y);

        ChangePosition(move);

        _setHorizontalForce.Invoke(deltaPosition.x);
        _setGrounded.Invoke(_grounded);

        _rigidbody.SetRotation(Vector2.SignedAngle(Vector2.up, _slopeNormal));
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private void ResetVerticalVelocity() => _velocity.y = -2f;

    private bool validateNextGroundNormal(Vector2 newNormal) =>
        newNormal != Vector2.zero && Vector2.Angle(_groundNormal, newNormal) < _maxNormalAngle;

    private void ChangePosition(Vector2 move, float maxRecursion = 1)
    {
        _hitDistance = Math.Max(_hitDistance - _groundOffset, 0);
        _rigidbody.velocity += _slopeNormal * (_hitDistance * Math.Sign(_velocity.y)) / Time.deltaTime;
        _hitDistance = 0;

        if (move == Vector2.zero)
            return;

        (var currentNormal, float distance, float _) =
            _functions.FindNearestNormal(move, move.magnitude, _bodyCollider, _contactFilter);

        var tail = move.magnitude - distance;
        _rigidbody.velocity += move * (distance / move.magnitude) / Time.deltaTime;

        if (maxRecursion <= 0 || tail < _groundOffset || _slopeNormal != _groundNormal)
            return;

        if (validateNextGroundNormal(currentNormal))
            _groundNormal = currentNormal;

        ChangePosition(CalculateMoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void UpdateGroundNormal(float verticalForce)
    {
        _lastGrounded = _grounded;
        _grounded = false;
        _leftLeg.isTrigger = true;
        _rightLeg.isTrigger = true;

        int dir = Math.Sign(verticalForce) == 0 ? -1 : Math.Sign(verticalForce);
        float castDistance = Math.Abs(verticalForce);

        var (leftNormal, lDistance, lHitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _leftLeg, _contactFilter);

        var (rightNormal, rDistance, rHitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _rightLeg, _contactFilter);

        var (normal, distance, hitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _bodyCollider, _contactFilter);

        var leftNormalIsValid = lHitCount > 0 && validateNextGroundNormal(leftNormal);
        var rightNormalIsValid = rHitCount > 0 && validateNextGroundNormal(rightNormal);

        if (leftNormalIsValid && !rightNormalIsValid)
        {
            _leftLeg.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: leftNormal, distance: lDistance, hitCount: lHitCount);
        }
        else if (rightNormalIsValid && !leftNormalIsValid)
        {
            _rightLeg.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: rightNormal, distance: rDistance, hitCount: rHitCount);
        }
        else
        {
            UpdateGroundNormal(force: verticalForce, normal: normal, distance: distance, hitCount: hitCount);
        }
    }

    private void UpdateGroundNormal(Vector2 normal, float distance, int hitCount, float force)
    {
        _hitDistance = distance;

        if (hitCount == 0)
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

            ResetVerticalVelocity();
            return;
        }

        _hitDistance = Math.Abs(force);
        _slopeNormal = new Vector2(-normal.y, normal.x);

        if (_slopeNormal.y < 0)
            _slopeNormal *= -1;

        if (_velocity.y < 0)
            return;

        if (Vector2.Angle(_slopeNormal, Vector2.right * Math.Sign(_slopeNormal.x)) > 45)
            return;

        _hitDistance = 0;
        ResetVerticalVelocity();
    }

    private void ChangeVelocityByNormal(Vector2 normal)
    {
        var reflect = Vector2.Reflect(_velocity.normalized, normal);
        var reflectVelocity = _velocity.RotateBy(Vector2.SignedAngle(_velocity.normalized, reflect));
        _velocity = (_velocity + reflectVelocity) / 2;
    }
}