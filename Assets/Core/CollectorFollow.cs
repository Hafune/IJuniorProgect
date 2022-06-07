using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class CollectorFollow : MonoBehaviour
{
    [SerializeField] private UnityEvent _coinCollected = null!;

    [CanBeNull] private Rigidbody2D _target;
    private Vector3 _velocity = Vector3.up * 30;
    private float _speed = .1f;
    private float _currentTime = 0f;
    private float _skipTime = .1f;

    public void FollowTo(Collider2D target) => _target = target.attachedRigidbody;

    private void Update()
    {
        if (_target == null)
            return;

        var lastPosition = new Vector2(transform.position.x, transform.position.y);
        transform.position += _velocity * Time.deltaTime;
        var newPosition = new Vector2(transform.position.x, transform.position.y);

        var targetPosition = _target.position;
        var modifySpeed = (targetPosition - lastPosition).sqrMagnitude >
                          (targetPosition - newPosition).sqrMagnitude
            ? _speed
            : _speed * 2f;
        _velocity.x += targetPosition.x > transform.position.x ? modifySpeed : -modifySpeed;
        _velocity.y += targetPosition.y > transform.position.y ? modifySpeed : -modifySpeed;
        _currentTime += Time.deltaTime;

        if ((targetPosition - newPosition).sqrMagnitude < .5f && _currentTime > _skipTime)
        {
            Destroy(gameObject);
            _coinCollected.Invoke();
        }
    }
}