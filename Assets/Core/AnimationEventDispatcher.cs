using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PhysicsMovement))]
public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int OnGround = Animator.StringToHash("onGround");

    [SerializeField] private Animator _animator = null!;
    [SerializeField] private SpriteRenderer _spriteRenderer = null!;
    [SerializeField] private PhysicsMovement _physicsMovement = null!;

    void Update()
    {
        if (_physicsMovement.Velocity.x != 0)
        {
            _animator.SetBool(IsMoving, true);

            _spriteRenderer.flipX = _physicsMovement.Velocity.x switch
            {
                < 0 => true,
                > 0 => false,
                _ => _spriteRenderer.flipX
            };
        }
        else
        {
            _animator.SetBool(IsMoving, false);
        }

        _animator.SetBool(OnGround, _physicsMovement.Grounded);
    }
}