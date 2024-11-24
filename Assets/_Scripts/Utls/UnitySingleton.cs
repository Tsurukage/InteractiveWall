using UnityEngine;

namespace Utls
{
    public class UnitySingleton<T> : MonoBehaviour where T : Component
    {
        static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
#if UNITY_6000_0_OR_NEWER
                instance = FindFirstObjectByType<T>();
#else
                instance = FindObjectOfType<T>();
#endif
                if (instance != null) return instance;
                var obj = new GameObject
                {
                    name = typeof(T).Name
                };
                instance = obj.AddComponent<T>();
                return instance;
            }
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            OnAwake();
        }
        protected virtual void OnAwake(){}
    }
}