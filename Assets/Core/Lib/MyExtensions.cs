using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Lib
{
    public static class MyExtensions
    {
        private static readonly Random _random = new Random();

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

        public static float Angle(this Vector2 vector) => (float) (Math.Atan2(vector.y, vector.x) * (180 / Math.PI));

        public static Vector3 Copy(this Vector3 vector, float? x = null, float? y = null, float? z = null) =>
            new Vector3(
                x ?? vector.x,
                y ?? vector.y,
                z ?? vector.z
            );

        public static Vector2 Copy(this Vector2 vector, float? x = null, float? y = null) =>
            new Vector2(
                x ?? vector.x,
                y ?? vector.y
            );

        public static Vector2 RotateBy(this Vector2 v, float a)
        {
            a *= Mathf.Deg2Rad;
            var ca = Math.Cos(a);
            var sa = Math.Sin(a);
            var rx = v.x * ca - v.y * sa;

            return new Vector2((float) rx, (float) (v.x * sa + v.y * ca));
        }

        public static Vector2 ReflectBy(this Vector2 vector, Vector2 normal)
        {
            var reflect = Vector2.Reflect(vector.normalized, normal);
            return vector.RotateBy(Vector2.SignedAngle(vector.normalized, reflect));
        }
    }
}