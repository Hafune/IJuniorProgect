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

    private const float ChangeDirectionValue = .01f;
    private const float SpeedToRun = .45f;
    private const float MaxMoveAnimationSpeed = 2.5f;
    private const float MaxRunAnimationSpeed = 4f;

    public void UpdateHorizontalVelocity(float force)
    {
        if (force < -ChangeDirectionValue)
            _spriteRenderer.flipX = true;
        else if (force > ChangeDirectionValue)
            _spriteRenderer.flipX = false;

        float absForce = Math.Abs(force);

        _animator.SetBool(IsMoving, absForce > ChangeDirectionValue);
        _animator.SetBool(IsRunning, absForce > SpeedToRun);

        if (_animator.GetBool(IsDamaged))
            _animator.speed = 1;
        else if (_animator.GetBool(IsRunning))
            _animator.speed = 1 + (absForce - SpeedToRun) / (1 - SpeedToRun) * MaxRunAnimationSpeed;
        else
            _animator.speed = 1 + absForce / SpeedToRun * MaxMoveAnimationSpeed;
    }

    public void UpdateGrounded(bool grounded) => _animator.SetBool(OnGround, grounded);

    public void SetDamaged(bool damaged) => _animator.SetBool(IsDamaged, damaged);
}