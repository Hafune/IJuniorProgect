using UnityEngine;
using UnityEngine.Events;

public class DamageVelocityBuilder : MonoBehaviour
{
    [SerializeField] private UnityEvent<Vector2> _dispatchVelocity;
    [SerializeField] private SpriteRenderer _renderer;

    private float horizontalPower = 3;
    private float verticalPower = 8;

    public void BuildVelocity() =>
        _dispatchVelocity.Invoke(new Vector2(_renderer.flipX ? horizontalPower : -horizontalPower, verticalPower));
}