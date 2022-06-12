using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _setPercent;
    [SerializeField] [Min(0f)] private float _maxPoints = 100f!;
    [SerializeField] [Min(0f)] private float _currentPoints = 100f!;

    public void TakeDamage(float value) => ChangePoints(-value);

    public void AddPoint(float value) => ChangePoints(value);

    private void ChangePoints(float value)
    {
        _currentPoints = Mathf.Clamp(_currentPoints + value, 0f, _maxPoints);
        _setPercent.Invoke(_currentPoints / _maxPoints);
    }
}