using UnityEngine;
using UnityEngine.Events;

public class CoinHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent _coinCollected;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out Coin coin))
            return;

        coin.Collected();
        _coinCollected.Invoke();
    }
}