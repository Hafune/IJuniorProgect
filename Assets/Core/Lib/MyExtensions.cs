using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lib
{
    public static class MyExtensions
    {
        // private static readonly Random _random = new Random();

        public static void RepeatTimes(this int count, Action callback)
        {
            for (int i = 0; i < count; i++)
            {
                callback.Invoke();
            }
        }

        public static void ForEachIndexed<T>(this List<T> list, Action<T, int> callback)
        {
            for (int i = 0; i < list.Count; i++)
            {
                callback.Invoke(list[i], i);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> callback)
        {
            foreach (var item in list)
            {
                callback.Invoke(item);
            }
        }

        public static float Angle(this Vector2 vector2) => (float) (Math.Atan2(vector2.y, vector2.x) * (180 / Math.PI));

        public static void SetAngle(ref this Vector2 velocity, float slopeAngle)
        {
            float magnitude = velocity.magnitude;
            velocity.y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * magnitude;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * magnitude;
        }

        private static float normalizeAngle(float angle)
        {
            const float max = 360f;

            if (angle is >= 0 and < max) return angle;

            angle %= max;

            if (angle < 0)
                angle += max;

            return angle;
        }
    }
}