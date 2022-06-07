using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [CanBeNull] [SerializeField] private Collider2D _leftWallChecker;
    [CanBeNull] [SerializeField] private Collider2D _leftGroundChecker;
    [CanBeNull] [SerializeField] private Collider2D _rightWallChecker;
    [CanBeNull] [SerializeField] private Collider2D _rightGroundChecker;
    [SerializeField] private float _moveForce;
    [SerializeField] private UnityEvent<Vector2> _setForce = null!;
    [SerializeField] private ContactFilter2D _contactWallFilter;
    [SerializeField] private ContactFilter2D _contactGroundFilter;

    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[1];
    private int _forceDirection = 1;

    private void FixedUpdate()
    {
        _forceDirection = (_moveForce * _forceDirection) switch
        {
            < 0 when HasContacts(_leftWallChecker, _contactWallFilter) ||
                     _leftGroundChecker &&
                     !HasContacts(_leftGroundChecker, _contactGroundFilter) => 1,
            > 0 when HasContacts(_rightWallChecker, _contactWallFilter) ||
                     _rightGroundChecker &&
                     !HasContacts(_rightGroundChecker, _contactGroundFilter) => -1,
            _ => _forceDirection
        };
        
        _setForce.Invoke(new Vector2(_moveForce * _forceDirection, 0));
    }

    private bool HasContacts([CanBeNull] Collider2D checker, ContactFilter2D contactFilter)
    {
        if (!checker)
            return false;

        return checker.Cast(Vector2.zero, contactFilter, _hitBuffer) > 0;
    }
}