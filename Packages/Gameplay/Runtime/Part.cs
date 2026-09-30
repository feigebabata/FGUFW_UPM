using System;
using System.Collections.Generic;
using FGUFW;
using UnityEngine;

namespace FGUFW.Gameplay
{
    public abstract class Part : MonoBehaviour
    {
        public bool PartEnabledFromParent => gameObject.activeInHierarchy;
        public bool PartEnabled
        {
            get
            {
                return gameObject.activeSelf;
            }
            private set
            {
                gameObject.SetActive(value);
            }
        }

        private readonly List<Part> subParts = new();
        private bool assetInstance = false;
        private bool initialized = false;

        protected void initializePart()
        {   
            if(initialized)return;

            foreach (Transform item in transform)
            {
                var subPart = item.GetComponent<Part>();
                if(subPart.IsNull()) continue;

                addPart(subPart);
            }

            OnCreatedPart();
            initialized = true;
            if(PartEnabled)
            {
                OnEnablePart();
            }
        }

        public void AddPart<T>(string partPrefabPath = default) where T : Part
        {
            if(GetPart<T>())return; // 已存在
            var partType = typeof(T);
            
            if(partPrefabPath.IsNull()) partPrefabPath = getPartPrefabPath(partType);
            var part = loadPart<T>(partPrefabPath);

            if(part==default)return;//加载失败

            addPart(part);
        }

        private T loadPart<T>(string partPrefabPath) where T : Part
        {
            var gObj = fg.assetLoader.Instantiate(partPrefabPath,transform);
            var part = gObj.GetComponent<T>();

            if(part.IsNull())
            {
                fg.assetLoader.ReleaseInstance(gObj);
                throw new InvalidOperationException($"Part预制件缺少组件: {typeof(T).FullName}");
            }
            else
            {
                part.assetInstance = true;
                return part;
            }
        }


        private void addPart(Part part)
        {
            if(!part)
            {
                throw new ArgumentNullException(nameof(part));
            }
            if(part == this)
            {
                throw new InvalidOperationException("Part不能添加自身");
            }
            if(subParts.Exists(p=>p.GetType()==part.GetType()))
            {
                throw new InvalidOperationException($"不允许并列相同Part: {part.GetType().Name}");
            }
            subParts.Add(part);
            if(!part.initialized)
            {
                part.initializePart();
            }
        }

        public T GetPart<T>() where T : Part
        {
            foreach (var subPart in subParts)
            {
                if(subPart is T part)
                {
                    return part;
                }
            }
            return default;
        }

        public void RemovePart<T>() where T : Part
        {
            var part = GetPart<T>();
            RemovePart(part);
        }

        public void RemovePart(Part part)
        {
            if(!part)return;
            if(!subParts.Remove(part))return;
            if(part.initialized && part.PartEnabled)
            {
                part.OnDisablePart();
            }
            part.OnDestroyPartRemoveAllSubPart();//子节点要在父节点清理之前清理
            part.OnDestroyPart();

            if(part.assetInstance)
            {
                fg.assetLoader.ReleaseInstance(part.gameObject);
            }
            else
            {
                Destroy(part.gameObject);
            }
        }

        private void OnDestroyPartRemoveAllSubPart()
        {
            var parts = new List<Part>(subParts);
            foreach (var subPart in parts)
            {
                RemovePart(subPart);
            }
        }

        /// <summary>
        /// 修改当前PartEnabled不会影响到SubPart 节点内自行调控
        /// </summary>
        /// <param name="enabled"></param>
        public void SetPartEnabled(bool enabled)
        {
            if(PartEnabled == enabled)return;
            PartEnabled = enabled;
            if(!initialized)return;
            if(enabled)
            {
                OnEnablePart();
            }
            else
            {
                OnDisablePart();
            }
        }

        protected void setAllSubPartEnable(bool enabled)
        {
            foreach (var subPart in subParts)
            {
                subPart.SetPartEnabled(enabled);
            }
        }


        protected abstract void OnCreatedPart();

        protected abstract void OnDestroyPart();

        protected abstract void OnEnablePart();

        protected abstract void OnDisablePart();

        private string getPartPrefabPath(Type partType)
        {
            return $"Gameplay.{partType.FullName}";
        }
    }
}
