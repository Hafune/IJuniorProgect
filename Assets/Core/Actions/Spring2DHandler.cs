using UnityEngine;
using UnityEngine.Events;

public class Spring2DHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector2> _dispatchVelocity;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.TryGetComponent(out Spring2D spring))
            return;

        _dispatchVelocity.Invoke(spring.Velocity);
    }
}