using UnityEngine;

namespace Utls
{
    public static class GameObjExtension
    {
        public const string UnityNull = "null";
        public static bool IsUnityNull(this object obj) => obj == null || obj.ToString() == UnityNull;
        public static bool IsInLayerMask(this GameObject obj,LayerMask mask) => (mask.value & (1 << obj.layer)) != 0;
    }
}