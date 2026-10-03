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

        public UIBase GetCurrentUI()
        {
            if(openStack.Count==0)return default;
            return openStack.Last();
        }

        /// <summary>
        /// 加载但不显示 结束后自动调用OnCreate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Proload<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);

            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                return uiBase as T;
            }
            else
            {
                return getOrLoadUI(uiBaseType) as T;
            }
        }

        public T Open<T>() where T : UIBase
        {
            var uiBase = getOrLoadUI(typeof(T));
            if(uiBase.IsNull())return default;

            // UICanvasSortingUtility.RegisterSort(uiBase.UICanvas);
            uiBase.OnOpen();
            openStack.Remove(uiBase);
            openStack.Add(uiBase);
            resetOverlayCanvasListOrder(new List<UIBase>(openStack));

            return uiBase as T;
        }

        public void Close<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);

            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                // UICanvasSortingUtility.UnregisterSort(uiBase.UICanvas);
                uiBase.OnClose();
                openStack.Remove(uiBase);
                resetOverlayCanvasListOrder(new List<UIBase>(openStack));
            }
        }

        /// <summary>
        /// UI对象使用这个接口销毁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Destroy<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);

            if(uiCache.TryGetValue(uiBaseType , out var uiBase))
            {
                uiCache.Remove(uiBaseType);
                // UICanvasSortingUtility.UnregisterSort(uiBase.UICanvas);
                openStack.Remove(uiBase);
                resetOverlayCanvasListOrder(new List<UIBase>(openStack));
                
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
                Debug.LogError($"UI预制件实例化失败: {key}");
                if(uiGObj)
                {
                    fg.assetLoader.ReleaseInstance(uiGObj);
                }
                Debug.LogException(exception);
                return default;
            }
        }

        private void setupUICanvas(UIBase uiBase)
        {
            uiBase.UICanvas = uiBase.GetComponent<Canvas>();
            uiBase.Group = uiBase.GetComponent<CanvasGroup>();
            uiBase.UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // uiBase.UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            // uiBase.UICanvas.worldCamera = UICamera;
            uiBase.UICanvas.sortingLayerName = uiBase.SortingLayer;
        }

        private string getUIPrefabKey(Type uiBaseType)
        {
            return $"UISystem.{uiBaseType.FullName}";
        }

        private void resetOverlayCanvasListOrder(List<UIBase> uiList)
        {
            uiList.Sort(compareOverlayCanvasOrder);

            for (int i = 0; i < uiList.Count; i++)
            {
                uiList[i].UICanvas.sortingOrder = i;
            }
        }

        private int compareOverlayCanvasOrder(UIBase lhs,UIBase rhs)
        {
            var lhsLayer = SortingLayer.GetLayerValueFromName(lhs.SortingLayer);
            var rhsLayer = SortingLayer.GetLayerValueFromName(rhs.SortingLayer);
            if(lhsLayer!=rhsLayer)return lhsLayer.CompareTo(rhsLayer);

            return lhs.UICanvas.sortingOrder.CompareTo(rhs.UICanvas.sortingOrder);
        }

    }
}
