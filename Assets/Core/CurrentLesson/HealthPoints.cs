using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthPoints : MonoBehaviour
{
    [SerializeField] private UnityEvent<float> _setPercent;
    [SerializeField][Min(0f)] private float _maxPoints = 100f!;
    [SerializeField][Min(0f)] private float _currentPoints = 100f!;

    public void IncrementPoints(float value)
    {
        _currentPoints = Mathf.Clamp(_currentPoints + value, 0f, _maxPoints);
        _setPercent.Invoke(_currentPoints / _maxPoints);
    }

    private void OnValidate() => IncrementPoints(0f);
}