using System.Collections;
using JetBrains.Annotations;
using Lib;
using UnityEngine;

public class HorizontalBar : MonoBehaviour
{
    [SerializeField] private float _maxWidth;
    [SerializeField] [Min(0)] private float _changeDelay = 0f;
    [SerializeField] [Min(0.0001f)] private float _changeSpeed = 1f;
    [SerializeField] [Range(0f, 1f)] private float _currentPercent = 1f;

    [CanBeNull] private Coroutine _enumerator;

    public void SetPercent(float percent)
    {
        _currentPercent = Mathf.Clamp(percent, 0f, 1f);

        if (_enumerator != null)
            StopCoroutine(_enumerator);

        _enumerator = StartCoroutine(AnimatePosition());
    }

    private void Start() => SetPercent(_currentPercent);

    private IEnumerator AnimatePosition()
    {
        float nextPositionX = -_maxWidth * (1f - _currentPercent);

        if (_changeDelay > 0 && nextPositionX < transform.localPosition.x)
            yield return new WaitForSeconds(_changeDelay);
        
        while (transform.localPosition.x != nextPositionX)
        {
            transform.localPosition = transform.localPosition.Copy(x: Mathf.MoveTowards(
                transform.localPosition.x,
                nextPositionX,
                _maxWidth * _changeSpeed * Time.deltaTime
            ));

            yield return null;
        }
    }
}