using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector2> _setForce = null!;

    private PlayerInput _playerInput = null!;

    private void Awake() => _playerInput = new PlayerInput();

    private void OnEnable() => _playerInput.Enable();

    private void OnDisable() => _playerInput.Disable();

    private float x;

    private void FixedUpdate()
    {
        var force = new Vector2(_playerInput.Player.Move.ReadValue<Vector2>().x, 0);

        if (_playerInput.Player.Jump.IsPressed())
            force.y = 1;

        // if (force.x != 0)
        // {
        //     if (x == force.x)
        //     {
        //         x = 0;
        //     }
        //     else
        //     {
        //         x = force.x;
        //     }
        // }
        // force.x = x;
        _setForce.Invoke(force);
    }
}