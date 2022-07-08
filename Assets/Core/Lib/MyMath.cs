using UnityEngine;

namespace Lib
{
    public class MyMath
    {
        public static float ClampBetween(float value, float first, float second) => first <= second
            ? Mathf.Clamp(value, first, second)
            : Mathf.Clamp(value, second, first);
    }
}