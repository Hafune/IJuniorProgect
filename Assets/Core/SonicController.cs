using System;
using Core;
using Lib;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SonicController : MonoBehaviour, IAnimationEvent
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector2 _velocity;

    private bool _grounded;
    private float _jumpForce = 10f;
    private float _moveForce = 8f;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Rigidbody2D _rigidbody = null!;
    private CircleCollider2D _circleCollider = null!;
    private BoxCollider2D _leftEdge = null!;
    private BoxCollider2D _rightEdge = null!;
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
        _circleCollider = GetComponent<CircleCollider2D>();
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;

        _leftEdge = transform.AddComponent<BoxCollider2D>();
        _rightEdge = transform.AddComponent<BoxCollider2D>();

        _leftEdge.isTrigger = true;
        _rightEdge.isTrigger = true;

        var size = new Vector2(_circleCollider.radius, _circleCollider.radius);
        _leftEdge.size = size;
        _rightEdge.size = size;

        var offset = -size / 2;
        _leftEdge.offset = offset;
        _rightEdge.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        _rigidbody.SetRotation(_groundNormal.Angle() - 90);
        _velocity.y += _targetVelocity.y * _jumpForce + Physics2D.gravity.y * Time.deltaTime;
        _velocity.x = _targetVelocity.x * _moveForce;
        _targetVelocity = Vector2.zero;
        _grounded = false;

        var deltaPosition = _velocity * Time.deltaTime;
        var move = MoveAlong(_groundNormal) * deltaPosition.x;

        UpdateGroundCollider(deltaPosition.y);
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

        (var currentNormal, float distance, float _) = FindNearestNormal(move, move.magnitude, _circleCollider);

        var tail = move.magnitude - distance;
        _rigidbody.position += move * (distance / move.magnitude);

        if (maxRecursion <= 0 || tail < hitDistanceOffset || _slopeNormal != _groundNormal)
            return;

        ChangeGroundNormal(currentNormal);
        ChangePosition(MoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void UpdateGroundCollider(float force)
    {
        _leftEdge.isTrigger = true;
        _rightEdge.isTrigger = true;

        int dir = Math.Sign(force) == 0 ? -1 : Math.Sign(force);

        var (lNormal, lDistance, lHitCount) =
            FindNearestNormal(_groundNormal * dir, Math.Abs(force), _leftEdge);

        var (rNormal, rDistance, rHitCount) =
            FindNearestNormal(_groundNormal * dir, Math.Abs(force), _rightEdge);

        var (normal, distance, hitCount) =
            FindNearestNormal(_groundNormal * dir, Math.Abs(force), _circleCollider);

        if (_grounded)
        {
            if (lHitCount > 0 && rHitCount == 0)
                ChangeGroundNormal(force: force, normal: lNormal, distance: lDistance, hitCount: lHitCount);

            if (lHitCount == 0 && rHitCount > 0)
                ChangeGroundNormal(force: force, normal: rNormal, distance: rDistance, hitCount: rHitCount);

            _leftEdge.isTrigger = lHitCount == 0;
            _rightEdge.isTrigger = rHitCount == 0;
        }
        else if (lNormal == _groundNormal && lHitCount > 0 && rHitCount == 0)
        {
            _leftEdge.isTrigger = false;
            ChangeGroundNormal(force: force, normal: lNormal, distance: lDistance, hitCount: lHitCount);
        }
        else if (rNormal == _groundNormal && rHitCount > 0 && lHitCount == 0)
        {
            _rightEdge.isTrigger = false;
            ChangeGroundNormal(force: force, normal: rNormal, distance: rDistance, hitCount: rHitCount);
        }
        else ChangeGroundNormal(force: force, normal: normal, distance: distance, hitCount: hitCount);
    }

    private void ChangeGroundNormalOld(float force, Collider2D currentCollider2D)
    {
        int dir = Math.Sign(force) == 0 ? -1 : Math.Sign(force);
        var (normal, distance, hitCount) =
            FindNearestNormal(_groundNormal * dir, Math.Abs(force), currentCollider2D);
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
            _velocity.y = -2;
            return;
        }

        hitDistance = Math.Abs(force);
        _slopeNormal = new Vector2(-normal.y, normal.x);

        if (_slopeNormal.y < 0)
            _slopeNormal *= -1;

        if (Vector2.Angle(_slopeNormal, Vector2.right * Math.Sign(_slopeNormal.x)) > 45)
            return;

        hitDistance = 0;
        _velocity.y = -2;
    }

    private void ChangeGroundNormal(Vector2 normal, float distance, int hitCount, float force)
    {
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
            _velocity.y = -2;
            return;
        }

        hitDistance = Math.Abs(force);
        _slopeNormal = new Vector2(-normal.y, normal.x);

        if (_slopeNormal.y < 0)
            _slopeNormal *= -1;

        if (Vector2.Angle(_slopeNormal, Vector2.right * Math.Sign(_slopeNormal.x)) > 45)
            return;

        hitDistance = 0;
        _velocity.y = -2;
    }

    private (Vector2 normal, float distance, int hitCount) FindNearestNormal(
        Vector2 direction,
        float castDistance,
        Collider2D currentCollider2D)
    {
        int count = currentCollider2D.Cast(direction, _contactFilter, _hitBuffer, castDistance);
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