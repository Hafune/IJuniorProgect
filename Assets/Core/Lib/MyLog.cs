using System.Linq;
using UnityEngine;

namespace Lib
{
    public static class MyLog
    {
        public static void Log(params object[] logs) =>
            Debug.Log(logs.Aggregate("", (s, o) => $"{s}, {o}"));
    }
}