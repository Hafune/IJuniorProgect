using UnityEngine;
using UnityEngine.Events;

public class AdditionalLayoutHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent<int> _dispatchLayer;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out AdditionalLayer layer))
            return;

        _dispatchLayer.Invoke(layer.Layer);
    }
}