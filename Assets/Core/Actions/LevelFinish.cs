using UnityEngine;
using UnityEngine.Events;

public class LevelFinish : MonoBehaviour
{
    [SerializeField] private UnityEvent _onFinish;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out PlayerController2D _))
            return;

        _onFinish.Invoke();
    }
}