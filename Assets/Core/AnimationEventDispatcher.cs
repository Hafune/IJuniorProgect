using System;
using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int IsDamaged = Animator.StringToHash("IsDamaged");

    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private const float changeDirectionValue = .01f;
    private const float speedToRun = .45f;
    private const float maxMoveAnimationSpeed = 2.5f;
    private const float maxRunAnimationSpeed = 4f;

    public void UpdateHorizontalVelocity(float force)
    {
        if (force < -changeDirectionValue)
            _spriteRenderer.flipX = true;
        else if (force > changeDirectionValue)
            _spriteRenderer.flipX = false;

        float absForce = Math.Abs(force);

        _animator.SetBool(IsMoving, absForce > changeDirectionValue);
        _animator.SetBool(IsRunning, absForce > speedToRun);

        if (_animator.GetBool(IsDamaged))
            _animator.speed = 1;
        else if (_animator.GetBool(IsRunning))
            _animator.speed = 1 + (absForce - speedToRun) / (1 - speedToRun) * maxRunAnimationSpeed;
        else
            _animator.speed = 1 + absForce / speedToRun * maxMoveAnimationSpeed;
    }

    public void UpdateGrounded(bool grounded) => _animator.SetBool(OnGround, grounded);

    public void SetDamaged(bool damaged) => _animator.SetBool(IsDamaged, damaged);
}