using UnityEngine;
using UnityEngine.Events;

public class BarController : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _incrementPoints;

    private PlayerInput _playerInput = null!;

    private void Awake() => _playerInput = new PlayerInput();

    private void OnEnable() => _playerInput.Enable();

    private void OnDisable() => _playerInput.Disable();

    private void Update()
    {
        if (!_playerInput.Player.Move.WasPressedThisFrame())
            return;

        float pointsPerStep = 10f;
        var nextPoints = _playerInput.Player.Move.ReadValue<Vector2>().x * pointsPerStep;

        _incrementPoints.Invoke(nextPoints);
    }
}