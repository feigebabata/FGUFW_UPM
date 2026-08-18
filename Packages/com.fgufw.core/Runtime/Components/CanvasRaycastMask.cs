using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FGUFW
{
        
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/CanvasRaycastMask")]
    public class CanvasRaycastMask : Graphic,ICanvasRaycastFilter
    {
        public Color FilterColor = new Color(0, 0, 0, 0);
        public float UpdateInterval = 0.1f; // 更新间隔
        public List<RectTransform> Filters = new();
        private float lastUpdateTime;

        private class Polygon
        {
            public List<Vector2> vertices = new List<Vector2>();
            public Color color;
            
            public Polygon(Rect rect, Color col)
            {
                vertices.Add(new Vector2(rect.xMin, rect.yMin));
                vertices.Add(new Vector2(rect.xMax, rect.yMin));
                vertices.Add(new Vector2(rect.xMax, rect.yMax));
                vertices.Add(new Vector2(rect.xMin, rect.yMax));
                color = col;
            }
        }

        void Update()
        {
            if(UpdateInterval<0)return;

            if (Time.time - lastUpdateTime >= UpdateInterval)
            {
                SetVerticesDirty();
                lastUpdateTime = Time.time;
            }
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            foreach (var item in Filters)
            {
                if(item.PointInside(sp,eventCamera)) return false;
            }
            return true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            
            if (Filters.Count == 0)
            {
                DrawPolygon(vh, new Polygon(rectTransform.rect, color));
                return;
            }
            
            // 获取当前多边形
            Polygon selfPolygon = new Polygon(rectTransform.rect, color);
            
            // 计算重叠区域
            List<Polygon> overlapPolygons = CalculateOverlapPolygons(selfPolygon);
            
            // 计算非重叠区域（当前多边形减去所有重叠区域）
            List<Polygon> nonOverlapPolygons = SubtractPolygons(selfPolygon, overlapPolygons);
            
            // 绘制非重叠区域
            foreach (Polygon polygon in nonOverlapPolygons)
            {
                DrawPolygon(vh, polygon);
            }
            
            // 绘制重叠区域
            for (int i = 0; i < overlapPolygons.Count; i++)
            {
                overlapPolygons[i].color = FilterColor;
                DrawPolygon(vh, overlapPolygons[i]);
            }
        }
        
        // 计算重叠多边形
        private List<Polygon> CalculateOverlapPolygons(Polygon selfPolygon)
        {
            List<Polygon> result = new List<Polygon>();
            
            foreach (RectTransform targetRect in Filters)
            {
                if (targetRect == null) continue;
                
                Rect targetLocalRect = GetLocalRect(targetRect);
                Polygon targetPolygon = new Polygon(targetLocalRect, Color.clear);
                
                // 计算两个多边形的交集
                Polygon overlap = PolygonIntersection(selfPolygon, targetPolygon);
                if (overlap.vertices.Count > 0)
                {
                    result.Add(overlap);
                }
            }
            
            return result;
        }
        
        // 多边形相减
        private List<Polygon> SubtractPolygons(Polygon main, List<Polygon> holes)
        {
            List<Polygon> result = new List<Polygon>();
            result.Add(main);
            
            foreach (Polygon hole in holes)
            {
                List<Polygon> newResult = new List<Polygon>();
                
                foreach (Polygon poly in result)
                {
                    newResult.AddRange(PolygonSubtraction(poly, hole));
                }
                
                result = newResult;
            }
            
            return result;
        }
        
        // 多边形交集（简化版）
        private Polygon PolygonIntersection(Polygon a, Polygon b)
        {
            // 简化：只处理矩形
            Rect rectA = GetBoundingRect(a.vertices);
            Rect rectB = GetBoundingRect(b.vertices);
            Rect overlap = RectIntersection(rectA, rectB);
            
            if (overlap.width <= 0 || overlap.height <= 0)
                return new Polygon(new Rect(0, 0, 0, 0), Color.clear);
                
            return new Polygon(overlap, Color.clear);
        }
        
        // 多边形相减（简化版）
        private List<Polygon> PolygonSubtraction(Polygon a, Polygon b)
        {
            List<Polygon> result = new List<Polygon>();
            
            // 简化：矩形相减
            Rect rectA = GetBoundingRect(a.vertices);
            Rect rectB = GetBoundingRect(b.vertices);
            
            if (!RectOverlap(rectA, rectB))
            {
                result.Add(a);
                return result;
            }
            
            Rect overlap = RectIntersection(rectA, rectB);
            
            // 分割成最多4个矩形
            // 左侧
            if (overlap.xMin > rectA.xMin)
            {
                result.Add(new Polygon(new Rect(
                    rectA.xMin, rectA.yMin,
                    overlap.xMin - rectA.xMin, rectA.height
                ), a.color));
            }
            
            // 右侧
            if (overlap.xMax < rectA.xMax)
            {
                result.Add(new Polygon(new Rect(
                    overlap.xMax, rectA.yMin,
                    rectA.xMax - overlap.xMax, rectA.height
                ), a.color));
            }
            
            // 下方
            if (overlap.yMin > rectA.yMin)
            {
                float xStart = Mathf.Max(rectA.xMin, overlap.xMin);
                float xEnd = Mathf.Min(rectA.xMax, overlap.xMax);
                if (xEnd > xStart)
                {
                    result.Add(new Polygon(new Rect(
                        xStart, rectA.yMin,
                        xEnd - xStart, overlap.yMin - rectA.yMin
                    ), a.color));
                }
            }
            
            // 上方
            if (overlap.yMax < rectA.yMax)
            {
                float xStart = Mathf.Max(rectA.xMin, overlap.xMin);
                float xEnd = Mathf.Min(rectA.xMax, overlap.xMax);
                if (xEnd > xStart)
                {
                    result.Add(new Polygon(new Rect(
                        xStart, overlap.yMax,
                        xEnd - xStart, rectA.yMax - overlap.yMax
                    ), a.color));
                }
            }
            
            return result;
        }
        
        // 绘制多边形
        private void DrawPolygon(VertexHelper vh, Polygon polygon)
        {
            if (polygon.vertices.Count < 3) return;
            
            int startIndex = vh.currentVertCount;
            
            // 添加顶点
            foreach (Vector2 vertex in polygon.vertices)
            {
                vh.AddVert(vertex, polygon.color, Vector2.zero);
            }
            
            // 三角剖分（简单多边形）
            for (int i = 1; i < polygon.vertices.Count - 1; i++)
            {
                vh.AddTriangle(startIndex, startIndex + i, startIndex + i + 1);
            }
        }
        
        // 辅助方法
        private Rect GetLocalRect(RectTransform target)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);
            
            for (int i = 0; i < 4; i++)
            {
                corners[i] = rectTransform.InverseTransformPoint(corners[i]);
            }
            
            float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
            float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        private Rect GetBoundingRect(List<Vector2> vertices)
        {
            if (vertices.Count == 0) return new Rect();
            
            float minX = vertices[0].x;
            float maxX = vertices[0].x;
            float minY = vertices[0].y;
            float maxY = vertices[0].y;
            
            foreach (Vector2 vertex in vertices)
            {
                minX = Mathf.Min(minX, vertex.x);
                maxX = Mathf.Max(maxX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                maxY = Mathf.Max(maxY, vertex.y);
            }
            
            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
        
        private Rect RectIntersection(Rect a, Rect b)
        {
            float xMin = Mathf.Max(a.xMin, b.xMin);
            float xMax = Mathf.Min(a.xMax, b.xMax);
            float yMin = Mathf.Max(a.yMin, b.yMin);
            float yMax = Mathf.Min(a.yMax, b.yMax);
            
            if (xMax <= xMin || yMax <= yMin)
                return new Rect(0, 0, 0, 0);
                
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        
        private bool RectOverlap(Rect a, Rect b)
        {
            return !(a.xMax <= b.xMin || a.xMin >= b.xMax ||
                    a.yMax <= b.yMin || a.yMin >= b.yMax);
        }
    }
}