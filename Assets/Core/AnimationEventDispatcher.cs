using System;
using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int OnGround = Animator.StringToHash("OnGround");

    [SerializeField] private Animator _animator = null!;
    [SerializeField] private SpriteRenderer _spriteRenderer = null!;
    private float changeDirectionValue = .01f;

    public void UpdateHorizontalVelocity(float force)
    {
        if (Math.Abs(force) > .02f)
        {
            _animator.SetBool(IsMoving, true);

            if (force < -changeDirectionValue)
                _spriteRenderer.flipX = true;
            else if (force > changeDirectionValue)
                _spriteRenderer.flipX = false;
        }
        else
        {
            _animator.SetBool(IsMoving, false);
        }
    }

    public void UpdateGrounded(bool grounded) => _animator.SetBool(OnGround, grounded);
}