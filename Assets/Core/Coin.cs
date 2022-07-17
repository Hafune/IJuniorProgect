using UnityEngine;
using UnityEngine.Events;

public class Coin : MonoBehaviour
{
    [SerializeField] private UnityEvent _onCollected;
    
    public void Collected()
    {
        if (!isActiveAndEnabled)
            return;
        
        _onCollected.Invoke();
        Destroy(gameObject);
    }
}