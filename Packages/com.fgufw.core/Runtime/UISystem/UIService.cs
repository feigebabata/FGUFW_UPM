using System;
using System.Collections.Generic;
using System.Linq;
using FGUFW;
using UnityEngine;

namespace FGUFW
{
    public partial class UIService
    {
        private Dictionary<Type,UIBase> uiCache = new();
        private List<UIBase> openStack=new();

        public T Open<T>() where T : UIBase
        {
            var uiBase = getOrLoadUI(typeof(T));
            if(uiBase.IsNull())return default;

            UICanvasSortingUtility.RegisterSort(uiBase.UICanvas);
            uiBase.OnOpen();
            openStack.Remove(uiBase);
            openStack.Add(uiBase);

            return uiBase as T;
        }

        public void Close<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);

            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                UICanvasSortingUtility.UnregisterSort(uiBase.UICanvas);
                uiBase.OnClose();
                openStack.Remove(uiBase);
            }
        }

        public void Destroy<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);

            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                uiCache.Remove(uiBaseType);
                UICanvasSortingUtility.UnregisterSort(uiBase.UICanvas);
                openStack.Remove(uiBase);
                
                fg.assetLoader.ReleaseInstance(uiBase.gameObject);
            }
        }

        private UIBase getOrLoadUI(Type uiBaseType)
        {
            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                return uiBase;
            }

            var key = getUIPrefabKey(uiBaseType);
            GameObject uiGObj = default;
            try
            {
                // 同步实例化 只适用于本地或已被缓存的资源 远程Bundle会阻塞主线程
                uiGObj = fg.assetLoader.Instantiate(key,UIRoot);
                if(!uiGObj)
                {
                    throw new Exception($"UI预制件实例化失败: {key}");
                }

                uiBase = uiGObj.GetComponent<UIBase>();
                if(!uiBaseType.IsInstanceOfType(uiBase))
                {
                    throw new Exception($"UI预制件类型错误: {key}, 需要 {uiBaseType.FullName}");
                }

                setupUICanvas(uiBase);
                uiBase.OnCreate();
                uiCache.Add(uiBaseType,uiBase);
                return uiBase;
            }
            catch (Exception exception)
            {
                if(uiGObj)
                {
                    fg.assetLoader.ReleaseInstance(uiGObj);
                }
                Debug.LogException(exception);
                return default;
            }
        }

        private string getUIPrefabKey(Type uiBaseType)
        {
            return $"UISystem.{uiBaseType.FullName}";
        }

        public UIBase GetCurrentUI()
        {
            if(openStack.Count==0)return default;
            return openStack.Last();
        }

    }
}
