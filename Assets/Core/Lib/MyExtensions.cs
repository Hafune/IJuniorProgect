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

        // public static T Random<T>(this List<T> list, Random? random = null)
        // {
        //     return list[random?.Next(list.Count) ?? _random.Next(list.Count)];
        // }

        public static float Angle(this Vector2 vector2) => (float) (Math.Atan2(vector2.y, vector2.x) * (180 / Math.PI));
    }
}