using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MyPhysics2D : MonoBehaviour
{
    private const float shellRadius = 0.01f;

    [SerializeField] private float _minGroundNormalY = .65f;
    [SerializeField] private float _gravityModifier = 1f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private Vector2 _velocity;

    public bool Grounded { get; private set; }
    public float HorizontalVelocity => _velocity.x;

    private float _jumpForce = 10f;
    private Vector2 _targetVelocity = Vector2.zero;
    private Vector2 _groundNormal;
    private Rigidbody2D _rigidbody;
    private ContactFilter2D _contactFilter;
    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private readonly List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

    public void SetForceX(float force) => _targetVelocity.x = force;

    public void TryJimp()
    {
        if (Grounded)
            _velocity.y = _jumpForce;
    }

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useLayerMask = true;
    }

    private void FixedUpdate()
    {
        _velocity += Physics2D.gravity * _gravityModifier * Time.deltaTime;
        _velocity.x = _targetVelocity.x * 5;

        Grounded = false;

        var deltaPosition = _velocity * Time.deltaTime;
        var moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
        var move = moveAlongGround * deltaPosition.x;

        ChangePosition(move, false);

        move = Vector2.up * deltaPosition.y;

        ChangePosition(move, true);
    }

    private void ChangePosition(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        int count = _rigidbody.Cast(move, _contactFilter, _hitBuffer, distance + shellRadius);

        _hitBufferList.Clear();

        for (int i = 0; i < count; i++)
        {
            _hitBufferList.Add(_hitBuffer[i]);
        }

        foreach (var hit2D in _hitBufferList)
        {
            var currentNormal = hit2D.normal;
            if (currentNormal.y > _minGroundNormalY)
            {
                Grounded = true;
                if (yMovement)
                {
                    _groundNormal = currentNormal;
                    currentNormal.x = 0;
                }
            }

            float projection = Vector2.Dot(_velocity, currentNormal);

            if (projection < 0)
            {
                _velocity -= projection * currentNormal;
            }

            float modifiedDistance = hit2D.distance - shellRadius;
            distance = modifiedDistance < distance ? modifiedDistance : distance;
        }

        _rigidbody.position += move.normalized * distance;
    }
}