using Lib;
using UnityEngine;

public class PhysicsFunctions
{
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[4];

    public (Vector2 normal, float distance, int hitCount) FindNearestNormal(
        Vector2 direction,
        float castDistance,
        Collider2D collider2D,
        ContactFilter2D _contactFilter
    )
    {
        int count = collider2D.Cast(direction, _contactFilter, _hitBuffer, castDistance, true);
        float distance = castDistance;
        int layer = collider2D.gameObject.layer;
        var normal = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            var hit2d = _hitBuffer[i];
            var currentDistance = hit2d.distance;

            if (currentDistance >= distance || Vector2.Dot(hit2d.normal, direction) > 0)
                continue;

            if (hit2d.transform.TryGetComponent(out PlatformEffector2D platform))
            {
                if (hit2d.distance == 0)
                    continue;

                var platformNormal = Vector2.up.RotateBy(platform.rotationalOffset);
                float angleDif = Vector2.Angle(-direction, platformNormal);
                float hitDif = Vector2.Angle(hit2d.normal, platformNormal);
                float halfSurfaceArc = platform.surfaceArc / 2;

                if (angleDif > halfSurfaceArc || hitDif > halfSurfaceArc)
                    continue;
            }

            distance = currentDistance;
            normal = hit2d.normal;
            layer = hit2d.transform.gameObject.layer;
        }

        return (normal, distance, layer);
    }
}