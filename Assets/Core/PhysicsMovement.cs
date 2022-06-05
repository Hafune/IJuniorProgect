using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsMovement : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _setForceX = null!;
    [SerializeField] private UnityEvent _tryJump = null!;

    private PlayerInput _playerInput = null!;

    private void Update()
    {
        _setForceX.Invoke(_playerInput.Player.Move.ReadValue<Vector2>().x);

        if (_playerInput.Player.Jump.IsPressed())
            _tryJump.Invoke();
    }

    private void OnEnable()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
    }

    private void OnDisable() => _playerInput.Disable();
}