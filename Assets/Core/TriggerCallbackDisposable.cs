using UnityEngine;
using UnityEngine.Events;

public class TriggerCallbackDisposable : MonoBehaviour
{
    [SerializeField] private UnityEvent<GameObject> _enterEvent = null!;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        
        _enterEvent?.Invoke(gameObject);
        Destroy(this);
    }
}