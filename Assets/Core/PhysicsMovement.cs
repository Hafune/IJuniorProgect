using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MyPhysics2D))]
public class PhysicsMovement : MonoBehaviour
{
    [SerializeField] private MyPhysics2D _myPhysics2D = null!;

    private PlayerInput _playerInput = null!;

    private void Update()
    {
        _myPhysics2D.SetForceX(_playerInput.Player.Move.ReadValue<Vector2>().x);

        if (_playerInput.Player.Jump.IsPressed())
            _myPhysics2D.TryJimp();
    }

    private void OnEnable()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
    }

    private void OnDisable() => _playerInput.Disable();
}