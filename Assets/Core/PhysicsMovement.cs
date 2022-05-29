using System.Collections.Generic;
using UnityEngine;

public class PhysicsMovement : MonoBehaviour
{
    public float MinGroundNormalY = .65f;
    public float GravityModifier = 1f;
    public Vector2 Velocity;
    public LayerMask LayerMask;

    public bool Grounded { get; private set; }

    private Vector2 _targetVelocity;
    private Vector2 _groundNormal;
    private Rigidbody2D _rigidbody;
    private ContactFilter2D _contactFilter;
    private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    private readonly List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

    private const float minMoveDistance = 0.001f;
    private const float shellRadius = 0.01f;

    private void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(LayerMask);
        _contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        _targetVelocity = new Vector2(Input.GetAxis("Horizontal"), 0);

        if (Input.GetKey(KeyCode.Space) && Grounded)
            Velocity.y = 10;
    }

    private void FixedUpdate()
    {
        Velocity += Physics2D.gravity * GravityModifier * Time.deltaTime;
        Velocity.x = _targetVelocity.x * 5;

        Grounded = false;

        var deltaPosition = Velocity * Time.deltaTime;
        var moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
        var move = moveAlongGround * deltaPosition.x;

        ChangePosition(move, false);

        move = Vector2.up * deltaPosition.y;

        ChangePosition(move, true);
    }

    private void ChangePosition(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = _rigidbody.Cast(move, _contactFilter, _hitBuffer, distance + shellRadius);

            _hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            foreach (var hit2D in _hitBufferList)
            {
                var currentNormal = hit2D.normal;
                if (currentNormal.y > MinGroundNormalY)
                {
                    Grounded = true;
                    if (yMovement)
                    {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(Velocity, currentNormal);

                if (projection < 0)
                {
                    Velocity -= projection * currentNormal;
                }

                float modifiedDistance = hit2D.distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        _rigidbody.position += move.normalized * distance;
    }
}