using UnityEngine;

namespace FGUFW
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;
        private static bool applicationIsQuitting;

        public static T I
        {
            get
            {
                if (applicationIsQuitting)
                {
                    return null;
                }

                if (instance == null)
                {
                    instance = GameObject.FindFirstObjectByType(typeof(T)) as T;
                    if (instance == null)
                    {
                        var gameObject = new GameObject(typeof(T).Name);
                        instance = gameObject.AddComponent<T>();
                    }
                }

                return instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            applicationIsQuitting = false;
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Debug.LogWarning($"Duplicate {typeof(T).Name} was destroyed.", this);
                Destroy(gameObject);
                return;
            }

            instance = this as T;
            if (IsDontDestroyOnLoad())
            {
                DontDestroyOnLoad(gameObject);
            }

            Init();
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            Dispose();
            instance = null;
        }

        protected virtual void Init()
        {
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }

        public virtual void Dispose()
        {
        }

        protected abstract bool IsDontDestroyOnLoad();

        public static bool NotNull()
        {
            return instance != null;
        }
    }
}
