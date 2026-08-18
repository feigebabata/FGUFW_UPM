using UnityEngine;
using System;
using UnityEngine.UI;

namespace FGUFW
{
    public static class GraphicExtensions
    {
        public static void DrawRect(this VertexHelper self, Rect rect, Color drawColor)
        {
            Vector3 bottomLeft = new Vector3(rect.xMin, rect.yMin, 0);
            Vector3 bottomRight = new Vector3(rect.xMax, rect.yMin, 0);
            Vector3 topLeft = new Vector3(rect.xMin, rect.yMax, 0);
            Vector3 topRight = new Vector3(rect.xMax, rect.yMax, 0);
            
            int startIndex = self.currentVertCount;
            
            self.AddVert(bottomLeft, drawColor, Vector2.zero);
            self.AddVert(bottomRight, drawColor, Vector2.zero);
            self.AddVert(topLeft, drawColor, Vector2.zero);
            self.AddVert(topRight, drawColor, Vector2.zero);
            
            self.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            self.AddTriangle(startIndex + 2, startIndex + 1, startIndex + 3);
        }
    }
}
