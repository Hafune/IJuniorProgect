using UnityEngine;

public class PhysicsFunctions
{
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[4];

    public (Vector2 normal, float distance, int hitCount) FindNearestNormal(
        Vector2 direction,
        float castDistance,
        Collider2D currentCollider2D,
        ContactFilter2D _contactFilter
    )
    {
        int count = currentCollider2D.Cast(direction, _contactFilter, _hitBuffer, castDistance);
        var distance = castDistance;
        var normal = Vector2.zero;

        int validHits = 0;

        for (int i = 0; i < count; i++)
        {
            var hit2D = _hitBuffer[i];
            var currentDistance = hit2D.distance;

            if (castDistance > currentDistance)
                validHits++;

            if (currentDistance >= distance)
                continue;

            distance = currentDistance;
            normal = hit2D.normal;
        }

        return (normal, distance, validHits);
    }
}