using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

namespace FGUFW
{
    public static class UGUIExtensions
    {
        public static void AddListener(this Button self,UnityAction call)
        {
            self.onClick.AddListener(call);
        }

        public static void RemoveListener(this Button self,UnityAction call)
        {
            self.onClick.RemoveListener(call);
        }

        public static void FitToRect(this BoxCollider2D self,RectTransform rectTransform)
        {
            if (rectTransform == null || self == null)return;

            // 1. 获取 RectTransform 的实际宽高
            Vector2 size = rectTransform.rect.size;

            // 2. BoxCollider2D 的 size 直接使用该尺寸
            self.size = size;

            // 3. 计算 Pivot 偏移，使 Collider 中心对齐视觉中心
            float pivotX = rectTransform.pivot.x;
            float pivotY = rectTransform.pivot.y;

            float offsetX = (0.5f - pivotX) * size.x;
            float offsetY = (0.5f - pivotY) * size.y;

            // 4. 设置 BoxCollider2D 的 offset（即中心点偏移）
            self.offset = new Vector2(offsetX, offsetY);

            // Debug.Log($"Pivot: ({pivotX}, {pivotY}), Offset: ({offsetX}, {offsetY})");
        }

        public static void FitToRect(this CapsuleCollider2D self,RectTransform rectTransform)
        {
            if (rectTransform == null || self == null)return;

            // 1. 获取 RectTransform 的实际宽高
            Vector2 size = rectTransform.rect.size;

            // 2. BoxCollider2D 的 size 直接使用该尺寸
            self.size = size;

            // 3. 计算 Pivot 偏移，使 Collider 中心对齐视觉中心
            float pivotX = rectTransform.pivot.x;
            float pivotY = rectTransform.pivot.y;

            float offsetX = (0.5f - pivotX) * size.x;
            float offsetY = (0.5f - pivotY) * size.y;

            // 4. 设置 BoxCollider2D 的 offset（即中心点偏移）
            self.offset = new Vector2(offsetX, offsetY);

            // Debug.Log($"Pivot: ({pivotX}, {pivotY}), Offset: ({offsetX}, {offsetY})");
        }

        
        public static void TryAddAllBtnListener(this MonoBehaviour self,MonoBehaviour listener)
        {
            var type = self.GetType();
            var fields = type.GetFieldsByCache(TypeExtensions.PUBLIC_MEMBER);
            var btnType = typeof(Button);

            foreach (var fieldInfo in fields)
            {
                if(fieldInfo.FieldType!=btnType)continue;

                var fieldName = fieldInfo.Name;
                var btnComp = fieldInfo.GetValue(self) as Button;
                
                var method = listener.GetType().GetMethod($"OnClick{fieldName}",BindingFlags.Public|BindingFlags.Instance|BindingFlags.NonPublic);
                if(method==default || method.GetParameters().Length>0)continue;

                var callback = Delegate.CreateDelegate(typeof(UnityAction),listener,method) as UnityAction;
                btnComp.AddListener(callback);
            }

        }

        public static void TryRemoveAllBtnListener(this MonoBehaviour self)
        {
            var type = self.GetType();
            var fields = type.GetFieldsByCache(TypeExtensions.PUBLIC_MEMBER);
            var btnType = typeof(Button);

            foreach (var fieldInfo in fields)
            {
                if(fieldInfo.FieldType!=btnType)continue;
                
                var btnComp = fieldInfo.GetValue(self) as Button;
                btnComp.onClick.RemoveAllListeners();
            }

        }

    }
}
