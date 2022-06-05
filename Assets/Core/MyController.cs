using System;
using Core;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MyController : MonoBehaviour, IAnimationEvent
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector2 _velocity;

    private bool _grounded;
    private float _jumpForce = 10f;
    private float _moveForce = 8f;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Rigidbody2D _rigidbody;
    private ContactFilter2D _contactFilter;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private float hitDistance = 0f;
    private float hitDistanceOffset = .004f;
    private float maxNormalAngle = 60f;

    public bool OnGround => _grounded;
    public float HorizontalVelocity => _velocity.x;

    public void SetForceX(float force) => _targetVelocity.x = force;

    public void TryJimp() => _targetVelocity.y = _grounded ? 1 : 0;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        _velocity.y += _targetVelocity.y * _jumpForce + Physics2D.gravity.y * Time.deltaTime;
        _velocity.x = _targetVelocity.x * _moveForce;
        _targetVelocity = Vector2.zero;
        _grounded = false;
        
        var deltaPosition = _velocity * Time.deltaTime;
        var move = MoveAlong(_groundNormal) * deltaPosition.x;

        ChangeGroundNormal(deltaPosition.y);
        ChangePosition(move);
    }

    private Vector2 MoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private bool ChangeGroundNormal(Vector2 newNormal)
    {
        _groundNormal = Vector2.Angle(_groundNormal, newNormal) < maxNormalAngle ? newNormal : _groundNormal;
        return newNormal == _groundNormal;
    }

    private void ChangePosition(Vector2 move, float maxRecursion = 1)
    {
        _rigidbody.position += _slopeNormal * (hitDistance * Math.Sign(_velocity.y));
        hitDistance = 0;

        if (move == Vector2.zero)
            return;

        (var currentNormal, float distance, float _) = FindNearestNormal(move, move.magnitude);

        var tail = move.magnitude - distance;
        _rigidbody.position += move * (distance / move.magnitude);

        if (maxRecursion <= 0 || tail < hitDistanceOffset || _slopeNormal != _groundNormal)
            return;

        ChangeGroundNormal(currentNormal);
        ChangePosition(MoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void ChangeGroundNormal(float force)
    {
        int dir = Math.Sign(force) == 0 ? -1 : Math.Sign(force);
        (var normal, float distance, float hitCount) = FindNearestNormal(_groundNormal * dir, Math.Abs(force));
        hitDistance = distance;

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
            _velocity.y = -1;
            return;
        }

        hitDistance = Math.Abs(force);
        _slopeNormal = new Vector2(-normal.y, normal.x);

        if (_slopeNormal.y < 0)
            _slopeNormal *= -1;

        if (Vector2.Angle(_slopeNormal, Vector2.right * Math.Sign(_slopeNormal.x)) > 45)
            return;

        hitDistance = 0;
        _velocity.y = Physics2D.gravity.y * Time.deltaTime;
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
            var currentDistance = Math.Min(Math.Max(hit2D.distance - hitDistanceOffset, 0), distance);

            if (currentDistance >= distance)
                continue;

            distance = currentDistance;
            normal = currentNormal;
        }

        return (normal, distance, count);
    }
}