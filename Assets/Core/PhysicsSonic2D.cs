using System;
using Lib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PhysicsSonic2D : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _dispatchHorizontalForce;
    [SerializeField] private UnityEvent<bool> _dispatchGrounded;

    private readonly PhysicsFunctions _functions = new PhysicsFunctions();
    private const float JumpScale = 12f;
    private const float MoveScale = 8f;
    private const float BaseMaxHorizontalSpeed = 20f;
    private const float TotalMaxHorizontalSpeed = 40f;
    private const float MaxVerticalSpeed = 40f;
    private const float StickySpeed = .3f;
    private const float GroundOffset = .01f;
    private const float MaxNextNormalAngle = 50f;
    private const float AccelerationTime = .0005f;
    private const float GroundFriction = .06f;
    private const float AirFriction = .003f;
    private bool _isGrounded;
    private bool _lastGrounded;
    private Rigidbody2D _rigidbody = null!;
    private CircleCollider2D _circleCollider = null!;
    private BoxCollider2D _leftLeg = null!;
    private BoxCollider2D _rightLeg = null!;
    private ContactFilter2D _contactFilter;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _targetVelocity;
    private Vector2 _velocity;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    public void SetVelocity(Vector2 velocity) =>
        _velocity = velocity.RotatedBySignedAngle(_groundNormal, Vector2.up);

    public void SetLayer(int layer)
    {
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(layer));
        gameObject.layer = layer;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _circleCollider = GetComponent<CircleCollider2D>();
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        _contactFilter.useTriggers = false;
        _contactFilter.useLayerMask = true;
        _groundNormal = Quaternion.Euler(0f, 0f, _rigidbody.rotation) * _groundNormal;

        _leftLeg = transform.AddComponent<BoxCollider2D>();
        _rightLeg = transform.AddComponent<BoxCollider2D>();

        _leftLeg.isTrigger = true;
        _rightLeg.isTrigger = true;

        const float circleBoxCollidersCastDifference = .01f;
        var size = _circleCollider.bounds.size;
        var boxSize = new Vector2(size.x * .49f, size.x / 2 - circleBoxCollidersCastDifference);
        _leftLeg.size = boxSize;
        _rightLeg.size = boxSize;

        var offset = -size / 4;
        offset.x += _circleCollider.offset.x;
        offset.y += _circleCollider.offset.y;
        offset.y += circleBoxCollidersCastDifference / 2;
        _leftLeg.offset = offset;
        _rightLeg.offset = new Vector2(-offset.x, offset.y);
    }

    private void FixedUpdate()
    {
        var gravity = _groundNormal * (Physics2D.gravity.y * Time.deltaTime);
        float friction = _velocity.x.Sign() * (_isGrounded ? GroundFriction : AirFriction);
        float speed = Math.Abs(_velocity.x);

        _velocity.x += _targetVelocity.x * MoveScale * (AccelerationTime / Time.deltaTime) - gravity.x;
        _velocity.x = MyMath.ClampBetween(_velocity.x - friction, _velocity.x, 0f);
        float totalSpeed = Math.Abs(_velocity.x);

        if (totalSpeed > BaseMaxHorizontalSpeed && totalSpeed > speed)
        {
            _velocity.x = speed * _velocity.x.Sign();
            var along = CalculateMoveAlong(_groundNormal) * _velocity.x.Sign();

            if (along.y < 0)
                _velocity.x += (totalSpeed - speed) * -along.y * _velocity.x.Sign();
        }

        _velocity.y += gravity.y;

        if (_isGrounded && _targetVelocity.y > 0)
            _velocity.y += _targetVelocity.y * JumpScale;

        _velocity.x = Math.Clamp(_velocity.x, -TotalMaxHorizontalSpeed, TotalMaxHorizontalSpeed);
        _velocity.y = Math.Clamp(_velocity.y, -MaxVerticalSpeed, MaxVerticalSpeed);
        _targetVelocity = Vector2.zero;
        _rigidbody.velocity = Vector2.zero;

        var deltaPosition = _velocity * Time.deltaTime;
        float deltaDistance = deltaPosition.magnitude + GroundOffset;

        if (_isGrounded && deltaPosition.y - GroundOffset < 0)
            CheckGroundNormal(-deltaDistance +
                              deltaDistance * Math.Min(_groundNormal.y, 0) * (1 - StickySpeed));
        else
            CheckGroundNormal(deltaPosition.y + (deltaPosition.y - GroundOffset).Sign() * GroundOffset);

        CheckWallNormal();

        var move = _velocity.RotatedBySignedAngle(Vector2.up, _groundNormal) * Time.deltaTime;

        ChangeBodyVelocity(move);

        _dispatchHorizontalForce.Invoke(_velocity.x / TotalMaxHorizontalSpeed);
        _dispatchGrounded.Invoke(_isGrounded);

        _rigidbody.SetRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    private void CheckWallNormal()
    {
        var deltaPosition = _velocity * Time.deltaTime;

        (var normal, float distance, _) =
            _functions.FindNearestNormal(CalculateMoveAlong(_groundNormal) * deltaPosition.x,
                Math.Abs(deltaPosition.x) + GroundOffset,
                _circleCollider, _contactFilter);

        if (normal != Vector2.zero && !IsValidNextGroundNormal(normal))
            _velocity.x = (distance - GroundOffset) * _velocity.x.Sign() / Time.deltaTime;
    }

    private void ChangeBodyVelocity(Vector2 move, float recursionsLeft = 5)
    {
        if (move == Vector2.zero)
            return;

        var nextMove = _rigidbody.velocity * Time.deltaTime + move;
        float deltaMagnitude = nextMove.magnitude;

        (var normal, float distance, _) =
            _functions.FindNearestNormal(nextMove, deltaMagnitude + GroundOffset,
                _circleCollider, _contactFilter);

        float totalMagnitude = distance - GroundOffset;
        var totalMove = nextMove * (totalMagnitude / deltaMagnitude);

        _rigidbody.velocity = totalMove / Time.deltaTime;

        if (recursionsLeft <= 0 || normal == Vector2.zero)
            return;

        var tailMove = nextMove - totalMove;

        if (!IsValidNextGroundNormal(normal))
        {
            _velocity = _velocity.ReflectedAlong(normal);
            tailMove = (tailMove + tailMove.ReflectedBy(normal)) / 2;
            ChangeBodyVelocity(tailMove, --recursionsLeft);
        }
        else
        {
            var tail = tailMove.ReflectedBy(normal);
            _groundNormal = normal;
            ChangeBodyVelocity(tail, --recursionsLeft);
        }
    }

    private void CheckGroundNormal(float verticalForce)
    {
        int dir = verticalForce.Sign() == 0 ? -1 : verticalForce.Sign();
        float castDistance = Math.Abs(verticalForce);

        (var leftNormal, float leftDistance, int leftLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _leftLeg, _contactFilter);

        (var rightNormal, float rightDistance, int rightLayer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _rightLeg, _contactFilter);

        (var normal, float distance, int layer) =
            _functions.FindNearestNormal(_groundNormal * dir, castDistance, _circleCollider, _contactFilter);

        bool centerNormalIsValid = normal != Vector2.zero;

        var leftNormalIsValid = leftDistance > 0 &&
                                IsValidNextGroundNormal(leftNormal,
                                    centerNormalIsValid && leftDistance > distance ? MaxNextNormalAngle : 1);
        var rightNormalIsValid = rightDistance > 0 &&
                                 IsValidNextGroundNormal(rightNormal,
                                     centerNormalIsValid && rightDistance > distance ? MaxNextNormalAngle : 1);

        if (leftNormalIsValid && !rightNormalIsValid)
            UpdateGroundNormal(normal: leftNormal, distance: leftDistance, layer: leftLayer);
        else if (rightNormalIsValid && !leftNormalIsValid)
            UpdateGroundNormal(normal: rightNormal, distance: rightDistance, layer: rightLayer);
        else
            UpdateGroundNormal(normal: normal, distance: distance, layer: layer);
    }

    private void UpdateGroundNormal(Vector2 normal, float distance, int layer)
    {
        _lastGrounded = _isGrounded;
        _isGrounded = false;

        if (normal == Vector2.zero)
        {
            if (_lastGrounded)
                _velocity = _velocity.RotatedBySignedAngle(Vector2.up, _groundNormal);

            _groundNormal = Vector2.up;
            return;
        }

        if (!IsValidNextGroundNormal(normal))
            return;

        _groundNormal = normal;
        _isGrounded = true;

        if (!_lastGrounded)
            _velocity = _velocity.ReflectedAlong(_groundNormal);

        _velocity.y = -(distance - GroundOffset) / Time.deltaTime;
        SetLayer(layer);
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private bool IsValidNextGroundNormal(Vector2 newNormal, float maxAngleDifference = MaxNextNormalAngle) =>
        newNormal != Vector2.zero && Vector2.Angle(_groundNormal, newNormal) < maxAngleDifference;
}