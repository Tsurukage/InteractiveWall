using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utls
{
    public static class DebugExtension
    {
        public static void Log(this Object obj, object message,LogType type,[CallerMemberName] string methodName = null)
        {
            var msg = message?.ToString();
            if (string.IsNullOrWhiteSpace(msg)) msg = "Invoke()!";
            var log = $"{obj.name} {methodName} : {msg}";
            switch (type)
            {
                case LogType.Error:
                    Debug.LogError(log);
                    break;
                case LogType.Assert:
                    Debug.Assert(false, log);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(log);
                    break;
                case LogType.Log:
                    Debug.Log(log);
                    break;
                case LogType.Exception:
                    throw new Exception(log);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void Log(this string message, Object obj, [CallerMemberName] string methodName = null) => obj.Log(message, LogType.Log, methodName);
        public static void Log(this Object obj, [CallerMemberName] string methodName = null) => obj.Log(string.Empty, LogType.Log, methodName);
        public static void Log(this string message, Object obj, LogType type, [CallerMemberName] string methodName = null) => obj.Log(message, type, methodName);
        public static void Log(this Object obj, LogType type, [CallerMemberName] string methodName = null) => obj.Log(string.Empty, type, methodName);
    }
}