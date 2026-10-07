using UnityEngine;

namespace FGUFW
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        public static T I
        {
            get
            {
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

        void Reset()
        {
            gameObject.name = typeof(T).Name;
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

    }
}
