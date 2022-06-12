using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsCharacter2D : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private UnityEvent<float> _setHorizontalForce;
    [SerializeField] private UnityEvent<bool> _setGrounded;

    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _slopeNormal = Vector2.up;
    private Collider2D _bodyCollider;
    private ContactFilter2D _contactFilter;
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private bool _grounded;
    private float _jumpScale = 10f;
    private float _moveScale = 8f;
    private float _hitDistance = 0f;
    private float _groundOffset = .004f;
    private float _maxNormalAngle = 60f;

    public bool OnGround => _grounded;
    public float HorizontalVelocity => _velocity.x;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    private void Start()
    {
        _bodyCollider = GetComponent<Collider2D>();
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
        _bodyCollider.attachedRigidbody.position += _slopeNormal * (_hitDistance * Math.Sign(_velocity.y));
        _hitDistance = 0;

        if (move == Vector2.zero)
            return;

        (var currentNormal, float distance, float _) = FindNearestNormal(move, move.magnitude, _bodyCollider);

        var tail = move.magnitude - distance;
        _bodyCollider.attachedRigidbody.position += move * (distance / move.magnitude);

        if (maxRecursion <= 0 || tail < _groundOffset || _slopeNormal != _groundNormal)
            return;

        TryChangeGroundNormal(currentNormal);
        ChangePosition(CalculateMoveAlong(_groundNormal) * (tail * Math.Sign(_velocity.x)), --maxRecursion);
    }

    private void UpdateGroundNormal(float force)
    {
        int dir = Math.Sign(force) == 0 ? -1 : Math.Sign(force);
        (var normal, float distance, float hitCount) =
            FindNearestNormal(_groundNormal * dir, Math.Abs(force), _bodyCollider);
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

    private (Vector2 normal, float distance, int hitCount) FindNearestNormal(
        Vector2 direction,
        float castDistance,
        Collider2D currentCollider2D)
    {
        int count = currentCollider2D.Cast(direction, _contactFilter, _hitBuffer, castDistance);
        var distance = castDistance;
        var normal = _groundNormal;

        int validHits = 0;

        for (int i = 0; i < count; i++)
        {
            var hit2D = _hitBuffer[i];
            var currentDistance = hit2D.distance;

            if (castDistance > currentDistance)
                validHits++;

            if (currentDistance >= distance)
                continue;

            distance = currentDistance;
            normal = hit2D.normal;
        }

        return (normal, distance, validHits);
    }
}