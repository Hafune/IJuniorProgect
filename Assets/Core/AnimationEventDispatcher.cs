using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MyPhysics2D))]
public class AnimationEventDispatcher : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int OnGround = Animator.StringToHash("onGround");

    [SerializeField] private Animator _animator = null!;
    [SerializeField] private SpriteRenderer _spriteRenderer = null!;
    [SerializeField] private MyPhysics2D _myPhysics2D = null!;

    private float changeDirectionValue = .01f;

    private void Update()
    {
        if (_myPhysics2D.HorizontalVelocity != 0)
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

        _animator.SetBool(OnGround, _myPhysics2D.Grounded);
    }
}