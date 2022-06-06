using System;
using Core;
using UnityEngine;

[RequireComponent(typeof(IAnimationEvent))]
public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int OnGround = Animator.StringToHash("onGround");

    [SerializeField] private Animator _animator = null!;
    [SerializeField] private SpriteRenderer _spriteRenderer = null!;
    private IAnimationEvent _myPhysics2D = null!;
    private float changeDirectionValue = .01f;

    private void Start() => _myPhysics2D = GetComponent<IAnimationEvent>();

    private void FixedUpdate()
    {
        if (Math.Abs(_myPhysics2D.HorizontalVelocity) > .02f)
        {
            _animator.SetBool(IsMoving, true);

            if (_myPhysics2D.HorizontalVelocity < -changeDirectionValue)
                _spriteRenderer.flipX = true;
            else if (_myPhysics2D.HorizontalVelocity > changeDirectionValue)
                _spriteRenderer.flipX = false;
        }
        else
        {
            _animator.SetBool(IsMoving, false);
        }

        _animator.SetBool(OnGround, _myPhysics2D.OnGround);
    }
}