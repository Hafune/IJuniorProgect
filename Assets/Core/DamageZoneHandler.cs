using UnityEngine;
using UnityEngine.Events;

public class DamageZoneHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent _onDamaged;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isActiveAndEnabled ||!col.TryGetComponent(out DamageZone zone))
            return;

        _onDamaged.Invoke();
    }
}