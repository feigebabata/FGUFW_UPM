using UnityEngine;
using System;

namespace FGUFW
{
    public static class RectTransformExtensions
    {
        public static void SetAnchoredY(this RectTransform self, float y)
        {
            var pos = self.anchoredPosition;
            pos.y = y;
            self.anchoredPosition = pos;
        }

        public static void SetAnchoredX(this RectTransform self, float x)
        {
            var pos = self.anchoredPosition;
            pos.x = x;
            self.anchoredPosition = pos;
        }
        public static void SetSizeY(this RectTransform self, float y)
        {
            var sizeDelta = self.sizeDelta;
            sizeDelta.y = y;
            self.sizeDelta = sizeDelta;
        }

        public static void SetSizeX(this RectTransform self, float x)
        {
            var sizeDelta = self.sizeDelta;
            sizeDelta.x = x;
            self.sizeDelta = sizeDelta;
        }

        public static void SetX(this Transform self, float x)
        {
            var pos = self.position;
            pos.x = x;
            self.position = pos;
        }

        public static void SetY(this Transform self, float y)
        {
            var pos = self.position;
            pos.y = y;
            self.position = pos;
        }

        public static void SetZ(this Transform self, float z)
        {
            var pos = self.position;
            pos.z = z;
            self.position = pos;
        }

        public static void SetLocalX(this Transform self, float x)
        {
            var pos = self.localPosition;
            pos.x = x;
            self.localPosition = pos;
        }

        public static void SetLocalY(this Transform self, float y)
        {
            var pos = self.localPosition;
            pos.y = y;
            self.localPosition = pos;
        }

        public static void SetLocalZ(this Transform self, float z)
        {
            var pos = self.localPosition;
            pos.z = z;
            self.localPosition = pos;
        }

        public static void SetAnchoredPosition(this RectTransform self,Canvas canvas,UnityEngine.EventSystems.PointerEventData eventData)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform.AsRT(),eventData.position,canvas.worldCamera, out position);
            // 更新UI元素的位置
            self.anchoredPosition = position;
        }

        /// <summary>
        /// 点在rect内
        /// </summary>
        public static bool PointInside(this RectTransform self,Vector2 point,Camera eventCamera)
        {
            if(!RectTransformUtility.ScreenPointToLocalPointInRectangle(self,point,eventCamera, out point))
            {
                return false;
            }

            return self.rect.Contains(point);

        }

        /// <summary>
        /// 计算两个矩形的重叠部分
        /// </summary>
        public static Rect GetOverlapRect(this Rect rect1, Rect rect2)
        {
            float xMin = Mathf.Max(rect1.xMin, rect2.xMin);
            float xMax = Mathf.Min(rect1.xMax, rect2.xMax);
            float yMin = Mathf.Max(rect1.yMin, rect2.yMin);
            float yMax = Mathf.Min(rect1.xMax, rect2.xMax);
            
            if (xMax < xMin || yMax < yMin)
                return new Rect(0, 0, 0, 0);
                
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public static Rect GetWorldRect(this RectTransform self)
        {
            Vector3[] corners = new Vector3[4];
            self.GetWorldCorners(corners);
            
            // 转换为屏幕空间
            Vector2 min = corners[0];
            Vector2 max = corners[2];
            
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        // 世界坐标转本地坐标
        public static Rect WorldToLocalRect(this RectTransform self,Rect worldRect)
        {
            Vector3 worldCenter = worldRect.center;
            Vector3 localCenter = self.InverseTransformPoint(worldCenter);
            
            return new Rect(
                localCenter.x - worldRect.width * 0.5f,
                localCenter.y - worldRect.height * 0.5f,
                worldRect.width,
                worldRect.height
            );
        }

    }
}
