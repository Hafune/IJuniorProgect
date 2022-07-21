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
    private const float _jumpScale = 12f;
    private const float _moveScale = 8f;
    private const float _baseMaxHorizontalSpeed = 20f;
    private const float _totalMaxHorizontalSpeed = 40f;
    private const float _maxVerticalSpeed = 40f;
    private const float _stickySpeed = _totalMaxHorizontalSpeed / 4f;
    private const float _groundOffset = .01f;
    private const float _maxNextNormalAngle = 50f;
    private const float _accelerationTime = .0005f;
    private const float _groundFriction = 0.06f;
    private const float _airFriction = _groundFriction * .01f;
    private bool _grounded;
    private bool _lastGrounded;
    private Rigidbody2D _rigidbody = null!;
    private CircleCollider2D _circleCollider = null!;
    private BoxCollider2D _leftLeg = null!;
    private BoxCollider2D _rightLeg = null!;
    private ContactFilter2D _contactFilter;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal = Vector2.up;
    private Vector2 _velocity;

    public void SetForce(Vector2 force) => _targetVelocity = force;

    public void SetVelocity(Vector2 velocity) =>
        _velocity = velocity.RotatedByAngleDifference(_groundNormal, Vector2.up);

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

        if (_grounded && deltaPosition.y - _groundOffset * 1.5 < 0)
            CheckGroundNormal(-deltaPosition.magnitude - _groundOffset);
        else
            CheckGroundNormal(deltaPosition.y + (deltaPosition.y - _groundOffset).Sign() * _groundOffset);

        CheckWallNormal();

        var move = _velocity.RotatedByAngleDifference(Vector2.up, _groundNormal) * Time.deltaTime;

        ChangeBodyVelocity(move);

        _dispatchHorizontalForce.Invoke(_velocity.x / _totalMaxHorizontalSpeed);
        _dispatchGrounded.Invoke(_grounded);

        _rigidbody.SetRotation(Vector2.SignedAngle(Vector2.up, _groundNormal));
    }

    private void CheckWallNormal()
    {
        var deltaPosition = _velocity * Time.deltaTime;

        (var normal, float distance, _) =
            _functions.FindNearestNormal(CalculateMoveAlong(_groundNormal) * deltaPosition.x,
                Math.Abs(deltaPosition.x) + _groundOffset,
                _circleCollider, _contactFilter);

        if (normal != Vector2.zero && !IsValidNextGroundNormal(normal))
            _velocity.x = (distance - _groundOffset) * _velocity.x.Sign() / Time.deltaTime;
    }

    private void ChangeBodyVelocity(Vector2 move, float recursionsLeft = 5)
    {
        if (move == Vector2.zero)
            return;

        var nextMove = _rigidbody.velocity * Time.deltaTime + move;
        float deltaMagnitude = nextMove.magnitude;

        (var normal, float distance, _) =
            _functions.FindNearestNormal(nextMove, deltaMagnitude + _groundOffset,
                _circleCollider, _contactFilter);

        float totalMagnitude = distance - _groundOffset;
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
                                    centerNormalIsValid && leftDistance > distance ? _maxNextNormalAngle : 1);
        var rightNormalIsValid = rightDistance > 0 &&
                                 IsValidNextGroundNormal(rightNormal,
                                     centerNormalIsValid && rightDistance > distance ? _maxNextNormalAngle : 1);

        if (leftNormalIsValid && !rightNormalIsValid)
            UpdateGroundNormal(normal: leftNormal, distance: leftDistance, layer: leftLayer);
        else if (rightNormalIsValid && !leftNormalIsValid)
            UpdateGroundNormal(normal: rightNormal, distance: rightDistance, layer: rightLayer);
        else
            UpdateGroundNormal(normal: normal, distance: distance, layer: layer);
    }

    private void UpdateGroundNormal(Vector2 normal, float distance, int layer)
    {
        _lastGrounded = _grounded;
        _grounded = false;

        if (normal == Vector2.zero)
        {
            if (_lastGrounded)
                _velocity = _velocity.RotatedByAngleDifference(Vector2.up, _groundNormal);

            _groundNormal = Vector2.up;
            return;
        }

        if (!IsValidNextGroundNormal(normal))
            return;

        _groundNormal = normal;
        _grounded = true;

        if (!_lastGrounded)
            _velocity = _velocity.ReflectedAlong(_groundNormal);

        _velocity.y = -(distance - _groundOffset) / Time.deltaTime;
        SetLayer(layer);
    }

    private Vector2 CalculateMoveAlong(Vector2 normal) => new Vector2(normal.y, -normal.x);

    private bool IsValidNextGroundNormal(Vector2 newNormal, float maxAngleDifference = _maxNextNormalAngle) =>
        newNormal != Vector2.zero && Vector2.Angle(_groundNormal, newNormal) < maxAngleDifference;
}