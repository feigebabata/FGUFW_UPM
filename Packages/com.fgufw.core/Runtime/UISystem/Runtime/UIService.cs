using System;
using System.Collections.Generic;
using System.Linq;
using FGUFW;
using UnityEngine;

namespace FGUFW.UISystem
{
    public partial class UIService
    {
        private Dictionary<Type,UIBase> uiCache = new();
        private HashSet<Type> uiCacheLoading = new ();
        private HashSet<Type> uiCacheLoadFailed = new();
        private Queue<OpenItem> uiOpenQueue = new();
        private bool uiOpening=false;
        private List<UIBase> openStack=new();



        public void Preload<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);
            if(!uiCache.ContainsKey(uiBaseType) && !uiCacheLoading.Contains(uiBaseType))
            {
                uiCacheLoadFailed.Remove(uiBaseType);
                loadUIPrefab(uiBaseType);
            }
        }

        public void OpenPush<T>(Action<T> complete) where T : UIBase
        {
            uiOpenQueue.Enqueue(new OpenItem<T>(complete));
            checkUIOpenQueue();
        }

        public void Close<T>() where T : UIBase
        {
            var uiBaseType = typeof(T);
            UIBase uiBase = default;

            if(uiCache.TryGetValue(uiBaseType , out uiBase))
            {
                UICanvasSortingUtility.UnregisterSort(uiBase.UICanvas);
                uiBase.OnClose();
                openStack.Remove(uiBase);
            }
        }

        private void checkUIOpenQueue()
        {
            if(uiOpening)return;

            if(uiOpenQueue.Count==0)return;

            var openItem = uiOpenQueue.Peek();

            UIBase uiBase = default;

            if(!uiCache.TryGetValue(openItem.UIBaseType , out uiBase))
            {
                if(uiCacheLoadFailed.Contains(openItem.UIBaseType))
                {
                    Debug.LogError($"UI加载失败: {getUIPrefabKey(openItem.UIBaseType)}");
                    uiOpenQueue.Dequeue();
                    checkUIOpenQueue();
                    return;
                }

                if(!uiCacheLoading.Contains(openItem.UIBaseType))
                {
                    loadUIPrefab(openItem.UIBaseType);
                }
                return;
            }

            uiOpening = true;
            uiOpenQueue.Dequeue();
            try
            {
                UICanvasSortingUtility.RegisterSort(uiBase.UICanvas);
                uiBase.OnOpen();
                openStack.Remove(uiBase);
                openStack.Add(uiBase);
                openItem.Complete(uiBase);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                uiOpening = false;
            }
            checkUIOpenQueue();
        }

        private async void loadUIPrefab(Type uiBaseType)
        {
            uiCacheLoading.Add(uiBaseType);
            var key = getUIPrefabKey(uiBaseType);
            GameObject uiGObj = default;
            try
            {
                uiGObj = await fg.assetLoader.InstantiateAsync(key,UIRoot);
                if(!uiGObj)
                {
                    throw new Exception($"UI预制件实例化失败: {key}");
                }

                UIBase uiBase = uiGObj.GetComponent<UIBase>();
                if(!uiBaseType.IsInstanceOfType(uiBase))
                {
                    Destroy(uiGObj);
                    throw new Exception($"UI预制件类型错误: {key}, 需要 {uiBaseType.FullName}");
                }

                setupUICanvas(uiBase);
                uiBase.OnCreate();
                uiCache.Add(uiBaseType,uiBase);
            }
            catch (Exception exception)
            {
                if(uiGObj)
                {
                    Destroy(uiGObj);
                }
                uiCacheLoadFailed.Add(uiBaseType);
                Debug.LogException(exception);
            }
            finally
            {
                uiCacheLoading.Remove(uiBaseType);
                checkUIOpenQueue();
            }
        }

        private string getUIPrefabKey(Type uiBaseType)
        {
            return $"UISystem.{uiBaseType.FullName}";
        }

        public UIBase GetCurrentUI()
        {
            if(uiOpenQueue.Count>0)return default;
            if(openStack.Count==0)return default;
            return openStack.Last();
        }

        abstract class OpenItem
        {
            public Type UIBaseType { get; }

            protected OpenItem(Type uiBaseType)
            {
                UIBaseType = uiBaseType;
            }

            public abstract void Complete(UIBase uiBase);
        }

        sealed class OpenItem<T> : OpenItem where T : UIBase
        {
            private readonly Action<T> complete;

            public OpenItem(Action<T> complete) : base(typeof(T))
            {
                this.complete = complete;
            }

            public override void Complete(UIBase uiBase)
            {
                complete?.Invoke((T)uiBase);
            }
        }

    }
}
