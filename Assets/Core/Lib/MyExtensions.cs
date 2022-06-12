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

        public static Vector3 Copy(this Vector3 vector, float x = float.NaN, float y = float.NaN, float z = float.NaN)
        {
            return new Vector3(
                float.IsNaN(x) ? vector.x : x,
                float.IsNaN(y) ? vector.y : y,
                float.IsNaN(z) ? vector.z : z
            );
        }
    }
}