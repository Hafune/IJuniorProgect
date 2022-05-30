using UnityEngine;
using UnityEngine.Events;

public class CoinCollect : MonoBehaviour
{
    [SerializeField] private UnityEvent _enterEvent = null!;
    [SerializeField] private UnityEvent _exitEvent = null!;

    private void OnTriggerEnter2D(Collider2D other) => _enterEvent?.Invoke();

    private void OnTriggerExit2D(Collider2D other) => _exitEvent?.Invoke();
}
