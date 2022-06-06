using System;
using Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsCharacter2D : MonoBehaviour, IAnimationEvent
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector2 _velocity;

    private bool _grounded;
    private float _jumpScale = 10f;
    private float _moveScale = 8f;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Rigidbody2D _rigidbody;
    private ContactFilter2D _contactFilter;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private float _hitDistance = 0f;
    private float _groundOffset = .004f;
    private float _maxNormalAngle = 60f;

    public bool OnGround => _grounded;
    public float HorizontalVelocity => _velocity.x;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        _velocity.y += Physics2D.gravity.y * Time.deltaTime;
        _velocity.x = _targetVelocity.x * _moveScale;

        if (_grounded)
            _velocity.y += _targetVelocity.y * _jumpScale;

        _grounded = false;
        _targetVelocity = Vector2.zero;

        var deltaPosition = _velocity * Time.deltaTime;
        var move = CalculateMoveAlong(_groundNormal) * deltaPosition.x;

        UpdateGroundNormal(deltaPosition.y);
        ChangePosition(move);
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private void ResetVerticalVelocity() => _velocity.y = -2f;

    private bool ChangeGroundNormal(Vector2 newNormal)
    {
        _groundNormal = Vector2.Angle(_groundNormal, newNormal) < _maxNormalAngle ? newNormal : _groundNormal;
        return newNormal == _groundNormal;
    }

    private void ChangePosition(Vector2 move, float maxRecursion = 1)
    {
        _hitDistance = Math.Max(_hitDistance - _groundOffset, 0);
        _rigidbody.position += _slopeNormal * (_hitDistance * Math.Sign(_velocity.y));
        _hitDistance = 0;

        if (move == Vector2.zero)
            return;

        (var currentNormal, float distance, float _) = FindNearestNormal(move, move.magnitude);

        var tail = move.magnitude - distance;
        _rigidbody.position += move * (distance / move.magnitude);

        if (maxRecursion <= 0 || tail < _groundOffset || _slopeNormal != _groundNormal)
            return;

        ChangeGroundNormal(currentNormal);
        ChangePosition(CalculateMoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void UpdateGroundNormal(float force)
    {
        int dir = Math.Sign(force) == 0 ? -1 : Math.Sign(force);
        (var normal, float distance, float hitCount) = FindNearestNormal(_groundNormal * dir, Math.Abs(force));
        _hitDistance = distance;

        if (hitCount == 0)
        {
            _groundNormal = Vector2.up;
            _slopeNormal = _groundNormal;
            return;
        }

        if (ChangeGroundNormal(normal))
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

        if (Vector2.Angle(_slopeNormal, Vector2.right * Math.Sign(_slopeNormal.x)) > 45)
            return;

        _hitDistance = 0;
        ResetVerticalVelocity();
    }

    private (Vector2 normal, float distance, int hitCount) FindNearestNormal(Vector2 direction, float castDistance)
    {
        int count = _rigidbody.Cast(direction, _contactFilter, _hitBuffer, castDistance);
        var distance = castDistance;
        var normal = _groundNormal;

        for (int i = 0; i < count; i++)
        {
            var hit2D = _hitBuffer[i];
            var currentNormal = hit2D.normal;
            var currentDistance = hit2D.distance;

            if (currentDistance >= distance)
                continue;

            distance = currentDistance;
            normal = currentNormal;
        }

        return (normal, distance, count);
    }
}