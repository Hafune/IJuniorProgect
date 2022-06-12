using UnityEngine;
using UnityEngine.Events;

public class BarController : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _setPercent;

    private PlayerInput _playerInput = null!;
    private float _maxPoints = 100f!;
    private float _currentPoints = 100f!;

    private void Awake() => _playerInput = new PlayerInput();

    private void OnEnable() => _playerInput.Enable();

    private void OnDisable() => _playerInput.Disable();

    private void Update()
    {
        if (!_playerInput.Player.Move.WasPressedThisFrame())
            return;

        float pointsPerStep = 10f;
        var nextPoints = _playerInput.Player.Move.ReadValue<Vector2>().x * pointsPerStep;
        _currentPoints = Mathf.Clamp(_currentPoints + nextPoints, 0f, _maxPoints);

        _setPercent.Invoke(_currentPoints / _maxPoints);
    }
}