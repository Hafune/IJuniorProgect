using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int OnGround = Animator.StringToHash("OnGround");
    private static readonly int IsDamaged = Animator.StringToHash("IsDamaged");

    [SerializeField] private Animator _animator;
    [SerializeField] private UnityEvent _onDamagedEnd;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private const float changeDirectionValue = .01f;
    private const float damagedTime = 1f;
    private const float speedToRun = .45f;

    public void UpdateHorizontalVelocity(float force)
    {
        if (_animator.GetBool(IsDamaged))
            return;

        if (force < -changeDirectionValue)
            _spriteRenderer.flipX = true;
        else if (force > changeDirectionValue)
            _spriteRenderer.flipX = false;

        float absForce = Math.Abs(force);
        var animationAddSpeed = Mathf.Max(0, absForce < speedToRun ? absForce * 4 : (absForce - speedToRun) * 8);
        _animator.speed = 1 + animationAddSpeed;

        _animator.SetBool(IsMoving, absForce > changeDirectionValue);
        _animator.SetBool(IsRunning, absForce > speedToRun);
    }

    public void UpdateGrounded(bool grounded)
    {
        if (_animator.GetBool(IsDamaged))
            return;

        _animator.SetBool(OnGround, grounded);
    }

    public void AnimateDamaged()
    {
        if (_animator.GetBool(IsDamaged))
            throw new Exception("Function TakeDamage should not be called");

        StartCoroutine(PlayAnimationDamaged());
    }

    private IEnumerator PlayAnimationDamaged()
    {
        _animator.SetBool(IsDamaged, true);

        yield return new WaitForSeconds(damagedTime);

        _animator.SetBool(IsDamaged, false);
        _onDamagedEnd.Invoke();
    }
}