using UnityEngine;

namespace FGUFW.Gameplay
{
    /// <summary>
    /// 业务功能的大模块单位
    /// </summary>
    public abstract class Play : Part
    {
        public static Play P {get;private set;}

        void Awake()
        {
            if(!P.IsNull())
            {
                Debug.LogError($"重复的Play实例: {GetType().Name}",this);
                Destroy(gameObject);
                return;
            }
            P = this;

            DontDestroyOnLoad(gameObject);

            initializePart();
        }

        /// <summary>
        /// 需要子类调用
        /// </summary>
        protected override void OnDestroyPart()
        {
            P = default;
        }

    }
}
