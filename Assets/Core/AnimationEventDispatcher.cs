using System;
using UnityEngine;

public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int OnGround = Animator.StringToHash("OnGround");

    [SerializeField] private Animator _animator = null!;
    [SerializeField] private SpriteRenderer _spriteRenderer = null!;
    private float changeDirectionValue = .01f;

    public void UpdateHorizontalVelocity(float force)
    {
        if (force < -changeDirectionValue)
            _spriteRenderer.flipX = true;
        else if (force > changeDirectionValue)
            _spriteRenderer.flipX = false;

        float absForce = Math.Abs(force);
        var animationAddSpeed = Mathf.Max(0, absForce < .45f ? absForce * 4 : (absForce - .45f) * 8);
        _animator.speed = 1 + animationAddSpeed;

        _animator.SetBool(IsMoving, absForce > .01f);
        _animator.SetBool(IsRunning, absForce > .45f);
    }

    public void UpdateGrounded(bool grounded) => _animator.SetBool(OnGround, grounded);
}