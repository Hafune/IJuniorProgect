using UnityEngine;
using UnityEngine.Events;

public class HealthInput : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _dealDamage;
    [SerializeField] private UnityEvent<float> _addHealth;

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

        if (nextPoints < 0)
            _dealDamage.Invoke(-nextPoints);
        else
            _addHealth.Invoke(nextPoints);
    }
}