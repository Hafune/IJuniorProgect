using UnityEngine;
using UnityEngine.Events;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector2> _setForce = null!;

    private PlayerInput _playerInput = null!;

    private void Awake() => _playerInput = new PlayerInput();

    private void OnEnable() => _playerInput.Enable();

    private void OnDisable() => _playerInput.Disable();

    private void FixedUpdate()
    {
        var force = new Vector2(_playerInput.Player.Move.ReadValue<Vector2>().x, 0);

        if (_playerInput.Player.Jump.IsPressed())
            force.y = 1;
        
        _setForce.Invoke(force);
    }
}