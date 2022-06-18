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
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private UnityEvent<float> _setHorizontalForce;
    [SerializeField] private UnityEvent<bool> _setGrounded;

    private Rigidbody2D _rigidbody = null!;
    private Collider2D _bodyCollider = null!;
    private BoxCollider2D _leftBox = null!;
    private BoxCollider2D _rightBox = null!;
    private ContactFilter2D _contactFilter;
    private readonly PhysicsFunctions _functions = new PhysicsFunctions();
    private const float _jumpScale = 10f;
    private const float _moveScale = 8f;
    private const float _maxSpeed = 40f;
    private const float _groundOffset = .004f;
    private const float _maxNormalAngle = 50f;
    private float _hitDistance = 0f;
    private bool _grounded;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _bodyCollider = GetComponent<Collider2D>();
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;
        _groundNormal = Quaternion.Euler(0f, 0f, _rigidbody.rotation) * _groundNormal;

        _leftBox = transform.AddComponent<BoxCollider2D>();
        _rightBox = transform.AddComponent<BoxCollider2D>();

        _leftBox.isTrigger = true;
        _rightBox.isTrigger = true;

        var size = _bodyCollider.bounds.size;
        var boxSize = new Vector2(size.x / 2, size.x / 2);
        _leftBox.size = boxSize;
        _rightBox.size = boxSize;

        var offset = -boxSize / 2;
        _leftBox.offset = offset;
        _rightBox.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        _rigidbody.SetRotation(_groundNormal.Angle() - 90);
        _velocity.y += Physics2D.gravity.y * Time.deltaTime;
        _velocity.x = _targetVelocity.x * _moveScale;

        if (_grounded)
            _velocity.y += _targetVelocity.y * _jumpScale;

        _targetVelocity = Vector2.zero;

        _velocity.x = Math.Clamp(_velocity.x, -_maxSpeed, _maxSpeed);
        _velocity.y = Math.Clamp(_velocity.y, -_maxSpeed, _maxSpeed);

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
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private void ResetVerticalVelocity() => _velocity.y = -2f;

    private bool TryChangeGroundNormal(Vector2 newNormal)
    {
        _groundNormal = Vector2.Angle(_groundNormal, newNormal) < _maxNormalAngle ? newNormal : _groundNormal;
        return newNormal == _groundNormal;
    }

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

        TryChangeGroundNormal(currentNormal);
        ChangePosition(CalculateMoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void UpdateGroundNormal(float verticalForce)
    {
        _grounded = false;
        _leftBox.isTrigger = true;
        _rightBox.isTrigger = true;

        int dir = Math.Sign(verticalForce) == 0 ? -1 : Math.Sign(verticalForce);
        float castDistance = Math.Abs(verticalForce);

        var (lNormal, lDistance, lHitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _leftBox, _contactFilter);

        var (rNormal, rDistance, rHitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _rightBox, _contactFilter);

        var (normal, distance, hitCount) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _bodyCollider, _contactFilter);

        if (lNormal == _groundNormal && lHitCount == 1 && rHitCount == 0)
        {
            _leftBox.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: lNormal, distance: lDistance, hitCount: lHitCount);
        }
        else if (rNormal == _groundNormal && rHitCount == 1 && lHitCount == 0)
        {
            _rightBox.isTrigger = false;
            UpdateGroundNormal(force: verticalForce, normal: rNormal, distance: rDistance, hitCount: rHitCount);
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
            _groundNormal = Vector2.up;
            _slopeNormal = _groundNormal;
            return;
        }

        if (TryChangeGroundNormal(normal))
        {
            _slopeNormal = _groundNormal;
            _grounded = true;
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
}