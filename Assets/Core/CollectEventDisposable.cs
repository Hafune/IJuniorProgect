using UnityEngine;
using UnityEngine.Events;

public class CollectEventDisposable : MonoBehaviour
{
    [SerializeField] private UnityEvent<Collider2D> _enterEvent = null!;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        
        _enterEvent?.Invoke(other);
        Destroy(this);
    }
}