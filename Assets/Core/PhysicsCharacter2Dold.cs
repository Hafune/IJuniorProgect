 using System;
 using System.Collections.Generic;
 using UnityEngine;

 [RequireComponent(typeof(Rigidbody2D))]
 public class PhysicsCharacter2Dold : MonoBehaviour
 {
     [SerializeField] private float _minGroundNormalY = .65f;
     [SerializeField] private LayerMask _layerMask;
     [SerializeField] private Vector2 _velocity;
     [SerializeField] private float friction = .1f;
     [SerializeField] private float speed = 1f;

     public bool Grounded { get; private set; }
     public float HorizontalVelocity => _velocity.x;

     private float _jumpForce = 10f;
     private Vector2 _targetVelocity = Vector2.zero;
     private Vector2 _groundNormal;
     private Rigidbody2D _rigidbody;
     private ContactFilter2D _contactFilter;
     private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
     private readonly List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);

     public void SetForceX(float force) => _targetVelocity.x = force * speed;

     public void TryJimp()
     {
         if (!Grounded) return;

         _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
         Grounded = false;
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
         _velocity = Physics2D.gravity;
         _velocity.x = _targetVelocity.x * 5;

         var deltaPosition = _velocity * Time.deltaTime;
         var moveAlongGround = Grounded ? new Vector2(_groundNormal.y, -_groundNormal.x) : Vector2.right;
         var move = moveAlongGround * deltaPosition.x;

         Grounded = false;

         ChangeVelocity(move);

         move = Vector2.up * deltaPosition.y;

         ChangeVelocity(move);

         float distance = move.magnitude;
         int count = _rigidbody.Cast(move, _contactFilter, _hitBuffer, distance);

         _hitBufferList.Clear();

         for (int i = 0; i < count; i++)
         {
             _hitBufferList.Add(_hitBuffer[i]);
         }

         float lastProjection = float.MaxValue;

         foreach (var hit2D in _hitBufferList)
         {
             var currentNormal = hit2D.normal;
             float projection = Vector2.Dot(_rigidbody.velocity, currentNormal);

             if (currentNormal.y > _minGroundNormalY && lastProjection > projection)
             {
                 Grounded = true;
                 lastProjection = projection;
                 _groundNormal = currentNormal;
             }
         }

         _rigidbody.sharedMaterial.friction = Grounded ? friction : 0f;
         //Unity bugs, just apply friction doesn't work
         _rigidbody.sharedMaterial = _rigidbody.sharedMaterial;

         if (deltaPosition.x == 0f && !Grounded)
         {
             _rigidbody.velocity = new Vector2(Mathf.MoveTowards(_rigidbody.velocity.x, 0f, .1f), _rigidbody.velocity.y);
         }
     }

     private void ChangeVelocity(Vector2 move) => _rigidbody.velocity =
         new Vector2(
             calcNextVelocityValue(move.x, _rigidbody.velocity.x),
             calcNextVelocityValue(move.y, _rigidbody.velocity.y)
         );


     private float calcNextVelocityValue(float value, float velocityValue)
     {
         float sum = value + velocityValue;

         if (Math.Abs(sum) < Math.Abs(velocityValue))
             return sum;

         if (Math.Abs(value) > Math.Abs(velocityValue))
         {
             float v = Math.Min(Math.Abs(value), Math.Abs(sum));
             return Math.Sign(sum) * v;
         }

         return velocityValue;
     }
}