using UnityEngine;

namespace FGUFW.Gameplay
{
    /// <summary>
    /// 业务功能的大模块单位
    /// </summary>
    public abstract class Play<T> : Part where T:Play<T>
    {
        public static T I {get;private set;}

        void Awake()
        {
            if(!I.IsNull())
            {
                Debug.LogError($"重复的Play实例: {GetType().Name}",this);
                Destroy(gameObject);
                return;
            }
            I = this as T;

            DontDestroyOnLoad(gameObject);

            initializePart();
        }

        protected override void OnDestroyPart()
        {
            I = default;
        }

        public void DestroyPlay()
        {
            OnDestroyPart();
            onDestroyPartRemoveAllSubPart();
            fg.assetLoader.ReleaseInstance(gameObject);
            Destroy(gameObject);
        }

    }
}
