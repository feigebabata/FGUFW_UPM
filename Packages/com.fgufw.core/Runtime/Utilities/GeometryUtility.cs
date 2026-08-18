using UnityEngine;
using System;
using System.Collections.Generic;

namespace FGUFW
{
    /// <summary>
    /// 几何助手
    /// </summary>
    public static class GeometryUtility
    {
        /// <summary>
        /// 多边形转三角形 顺时针 法线向内 
        /// n个顶点的多边形可以切成n-2个三角形
        /// </summary>
        // public unsafe static int[] Polygon2Triangles(Vector3[] vertices,Vector3 normal,ref bool error,int[] triangles=null)
        // {
        //     if(vertices==null || vertices.Length<3)return null;

        //     int triangleLength = (vertices.Length-2)*3;
        //     if(triangles==null || triangles.Length!=triangleLength)triangles=new int[triangleLength];
        //     int triangleIndex = 0;
        //     var polygonLength = vertices.Length;
        //     var polygonPointer = stackalloc Vector3[polygonLength];
        //     var polygonIndex = stackalloc int[polygonLength];
        //     var polygon = new StackPolygon(polygonPointer,polygonIndex,vertices,normal.normalized);
        //     error = false;
        //     while (!polygon.ConvexPolygon())
        //     {
        //         error = true;
        //         for (int i = 0; i < polygon.Length; i++)
        //         {
        //             //切耳朵
        //             if(polygon.CanCutConcaveAngle(i))
        //             {
        //                 polygon.CutConvexAngle(i,triangles,ref triangleIndex);
        //                 error = false;
        //                 break;
        //             }
        //         }
        //         if(error)break;
        //     }
        //     polygon.SqlitConvex(triangles,ref triangleIndex);
            
        //     return triangles;
        // }


        // private unsafe struct StackPolygon
        // {
        //     private Vector3* _vertices;
        //     private int* _verticeIndexs;
        //     private Vector3 _normal;
        //     public int Length{get;private set;}
        //     public StackPolygon(Vector3* vertices,int* verticeIndexs,Vector3[] polygon,Vector3 normal)
        //     {
        //         _vertices = vertices;
        //         _verticeIndexs = verticeIndexs;
        //         Length = polygon.Length;
        //         for (int i = 0; i < Length; i++)
        //         {
        //             vertices[i] = polygon[i];
        //             verticeIndexs[i] = i;
        //         }
        //         _normal = normal;
        //     }

        //     /// <summary>
        //     /// 凸多边形
        //     /// </summary>
        //     /// <returns></returns>
        //     public bool ConvexPolygon()
        //     {
        //         for (int i = 0; i < Length; i++)
        //         {
        //             if(IsConcaveAngle(i))
        //             {
        //                 return false;
        //             }
        //         }
        //         return true;
        //     }
            
        //     /// <summary>
        //     /// 凹顶点
        //     /// </summary>
        //     public bool IsConcaveAngle(int vertexIndex)
        //     {
        //         var (v1,v2) = getVector(vertexIndex);

        //         // 计算该内角的外向法线
        //         Vector3 normal = Vector3.Cross(v1, v2).normalized;

        //         var val = Vector3.Dot(normal, _normal);
        //         return val < 0;
        //     }

        //     /// <summary>
        //     /// 能否切去耳朵
        //     /// </summary>
        //     /// <param name="vertexIndex"></param>
        //     /// <returns></returns>
        //     public bool CanCutConcaveAngle(int vertexIndex)
        //     {
        //         if(!IsConcaveAngle(vertexIndex))return false;
        //         int l_idx = (vertexIndex-1).RoundIndex(Length);
        //         int r_idx = (vertexIndex+1).RoundIndex(Length);
        //         return !IsConcaveAngle(l_idx) || !IsConcaveAngle(r_idx);
        //     }

        //     ValueTuple<Vector3,Vector3> getVector(int vertexIndex)
        //     {
        //         // 计算该内角对应的两个相邻顶点的向量
        //         Vector3 v1 = _vertices[(vertexIndex + Length - 1) % Length] - _vertices[vertexIndex];
        //         Vector3 v2 = _vertices[(vertexIndex + 1) % Length] - _vertices[vertexIndex];
        //         return (v1,v2);
        //     }

        //     /// <summary>
        //     /// 切去凹角旁边的凸角
        //     /// </summary>
        //     public void CutConvexAngle(int vertexIndex,int[] triangles,ref int triangleIndex)
        //     {
        //         int l_idx = (vertexIndex-1).RoundIndex(Length);
        //         int r_idx = (vertexIndex+1).RoundIndex(Length);
        //         if(!IsConcaveAngle(l_idx))
        //         {
        //             vertexIndex-=2;
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex++).RoundIndex(Length)];
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex++).RoundIndex(Length)];
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex++).RoundIndex(Length)];
        //             RemoveVertex(l_idx);
        //         }
        //         else if(!IsConcaveAngle(r_idx))
        //         {
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex--).RoundIndex(Length)];
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex--).RoundIndex(Length)];
        //             triangles[triangleIndex++] = _verticeIndexs[(vertexIndex--).RoundIndex(Length)];
        //             RemoveVertex(r_idx);
        //         }
        //     }

        //     public void RemoveVertex(int vertexIndex)
        //     {
        //         if(vertexIndex>=Length)return;
        //         Length--;
        //         for (int i = vertexIndex; i < Length; i++)
        //         {
        //             _vertices[i]=_vertices[i+1];
        //             _verticeIndexs[i]=_verticeIndexs[i+1];
        //         }
        //     }

        //     /// <summary>
        //     /// 切割凸多边形
        //     /// </summary>
        //     public void SqlitConvex(int[] triangles,ref int triangleIndex)
        //     {
        //         if(Length<3)return;
        //         for (int i = 1; i < Length-1; i++)
        //         {
        //             triangles[triangleIndex++] = _verticeIndexs[0];
        //             triangles[triangleIndex++] = _verticeIndexs[(i).RoundIndex(Length)];
        //             triangles[triangleIndex++] = _verticeIndexs[(i+1).RoundIndex(Length)];
        //         }
        //     }

        //     // static Vector3 centerPoint(Vector3* vertices,int length)
        //     // {
        //     //     // 计算多边形的中心点
        //     //     Vector3 center = Vector3.zero;
        //     //     for (int i = 0; i < length; i++)
        //     //     {
        //     //         center += vertices[i];
        //     //     }
        //     //     center /= length;
        //     //     return center;
        //     // }
        // }

        

        /// <summary>
        /// 点在三角形内
        /// </summary>
        /// <param name="point"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool InTriangle(Vector3 point,Vector3 a,Vector3 b,Vector3 c)
        {
            Vector3 pa = a - point;
            Vector3 pb = b - point;
            Vector3 pc = c - point;
            Vector3 pab = Vector3.Cross(pa,pb);
            Vector3 pbc = Vector3.Cross(pb, pc);
            Vector3 pca = Vector3.Cross(pc, pa);
            
            float d1 = Vector3.Dot(pab, pbc);
            float d2 = Vector3.Dot(pab, pca);
            float d3 = Vector3.Dot(pbc, pca);
 
            if (d1 > 0 && d2 > 0 && d3 > 0) return true;
            return false;
        }

        /// <summary>
        /// 点是否在椭圆内 水平方向 无旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="center">中心点</param>
        /// <param name="width">椭圆宽</param>
        /// <param name="height">椭圆高</param>
        /// <returns></returns>
        // public static bool PointInEllipse(float3 point,float3 center,float width,float height)
        // {
        //     float a = width/2;
        //     float b = height/2;

        //     float X = center.x;
        //     float Y = center.y;
        //     float x = point.x;
        //     float y = point.y;

        //     float cc = ((x-X)*(x-X)) / (a*a) + ((y-Y)*(y-Y)) / (b*b);

        //     return cc<=1f;
        // }
        

        /// <summary>
        /// 椭圆形2D
        /// </summary>
        /// <param name="center"></param>
        /// <param name="width">长半轴</param>
        /// <param name="height">短半轴</param>
        /// <param name="rotation"></param>
        /// <param name="pointCount"></param>
        /// <returns></returns>
        public static Vector2[] Ellipse(Vector2 center, float width, float height, float rotation,int pointCount)
        {
            Vector2[] points = new Vector2[pointCount];
            for (float i = 0; i < pointCount; i++)
            {
                float rate = i/pointCount;
                float angle = 2*Mathf.PI*rate;
                points[(int)i] = getPointOnEllipse(center,width,height,angle,rotation);
            }
            return points;
        }

        /// <summary>
        /// 椭圆边上的点
        /// </summary>
        /// <param name="center"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        static private Vector2 getPointOnEllipse(Vector2 center, float width, float height, float angle, float rotation)
        {
            float dLiXin = Mathf.Atan2(width*Mathf.Sin(angle), height*Mathf.Cos(angle));//离心角
            float x = width*Mathf.Cos(dLiXin)*Mathf.Cos(rotation) - height*Mathf.Sin(dLiXin)*Mathf.Sin(rotation) + center.x;
            float y = width*Mathf.Cos(dLiXin)*Mathf.Sin(rotation) + height*Mathf.Sin(dLiXin)*Mathf.Cos(rotation) + center.y;
            return new Vector2(x, y);
        }

        
        /// <summary>
        /// 遍历三维格子点 
        /// X->Z->Y
        /// </summary>
        public static void SpaceForeachGrid(Vector3 space_min,Vector3 space_max,Vector3Int girdCount,Vector3 pivot,Action<Vector3Int,int,Vector3> callback)
        {
            var space_size = space_max-space_min;
            Vector3 gridSize = VectorUtility.Division(space_size,girdCount);

            Vector3 pivotOffset = VectorUtility.Multiply(gridSize,pivot);
            int index = 0;
            for (int y_idx = 0; y_idx < gridSize.y; y_idx++)
            {
                for (int z_idx = 0; z_idx < gridSize.z; z_idx++)
                {
                    for (int x_idx = 0; x_idx < gridSize.x; x_idx++)
                    {
                        var coord = new Vector3Int(x_idx,y_idx,z_idx);
                        var point = SpaceCoord2Point(coord,space_min,gridSize,pivotOffset);
                        callback(coord,index++,point);
                    }
                }
            }
        }

        /// <summary>
        /// 空间索引的位置
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="space_min"></param>
        /// <param name="gridSize"></param>
        /// <param name="pivotOffset"></param>
        /// <returns></returns>
        public static Vector3 SpaceCoord2Point(Vector3Int coord,Vector3 space_min,Vector3 gridSize,Vector3 pivotOffset)
        {
            Vector3 point = space_min + VectorUtility.Multiply(coord,gridSize)+pivotOffset;
            return point;
        }

        /// <summary>
        /// 点在空间内
        /// </summary>
        /// <param name="space_min"></param>
        /// <param name="space_max"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool SpacePointInside(Vector3 space_min,Vector3 space_max,Vector3 point)
        {
            if(point.x<space_min.x || point.x>=space_max.x)return false;
            if(point.y<space_min.y || point.y>=space_max.y)return false;
            if(point.z<space_min.z || point.z>=space_max.z)return false;
            return true;
        }

        /// <summary>
        /// 坐标在空间内
        /// </summary>
        /// <param name="gridCount"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        public static bool SpaceCoordInside(Vector3Int gridCount,Vector3Int coord)
        {
            if(coord.x<0 || coord.x>=gridCount.x)return false;
            if(coord.y<0 || coord.y>=gridCount.y)return false;
            if(coord.z<0 || coord.z>=gridCount.z)return false;
            return true;
        }


        /// <summary>
        /// 空间格子索引 
        /// X->Z->Y
        /// </summary>
        public static int SpaceCoord2Index(Vector3Int gridCount,Vector3Int coord)
        {
            int idx = -1;
            if(!SpaceCoordInside(gridCount,coord))return idx;
            idx = coord.y*(gridCount.x*gridCount.z) + coord.z*gridCount.x + coord.x;
            return idx;
        }

        /// <summary>
        /// 点在空间的索引
        /// </summary>
        public static int SpacePoint2Index(Vector3 space_min,Vector3 space_max,Vector3 point,Vector3Int gridCount)
        {
            int idx = -1;
            idx = SpaceCoord2Index(gridCount,SpacePoint2Coord(space_min,space_max,point,gridCount));
            return idx;
        }

        /// <summary>
        /// 点在空间的坐标
        /// </summary>
        public static Vector3Int SpacePoint2Coord(Vector3 space_min,Vector3 space_max,Vector3 point,Vector3Int gridCount)
        {
            var spaceSize = space_max-space_min;
            int coord_x = MathUtility.IndexOf(gridCount.x,point.x,spaceSize.x,space_min.x);
            int coord_y = MathUtility.IndexOf(gridCount.y,point.y,spaceSize.y,space_min.y);
            int coord_z = MathUtility.IndexOf(gridCount.z,point.z,spaceSize.z,space_min.z);

            return new Vector3Int(coord_x,coord_y,coord_z);
        }

#region 六边形
/*
尖朝上的六边形 
*/
        public const float HEX_RADIUS_OUT2INN = 0.866025404f;

        /// <summary>
        /// 生成六边形
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="outRadius">外径</param>
        /// <param name="vectorsCache">顶点缓存 length=6</param>
        public static void GenerateHex(Vector2 center,float outRadius,Vector2[] vectorsCache)
        {
            const int length = 6;
            if(vectorsCache?.Length!=length)return;

            for (int i = 0; i < length; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                vectorsCache[i] = center + new Vector2(outRadius * Mathf.Sin(angle),outRadius * Mathf.Cos(angle));
            }
        }

        /// <summary>
        /// 局部坐标转六边形索引
        /// </summary>
        /// <param name="pointInHexLocalPosition"></param>
        /// <param name="outRadius"></param>
        /// <returns></returns>
        public static Vector3Int PointInHexIndex(Vector2 pointInHexLocalPosition,float outRadius)
        {
            float innRadius = outRadius*HEX_RADIUS_OUT2INN;

            float point_x = pointInHexLocalPosition.x;
            float point_y = pointInHexLocalPosition.y;

            float index_x = point_x / (innRadius*2);
            float index_y = -index_x;

            float offset = point_y / (outRadius*3);
            index_x -= offset;
            index_y -= offset;

            int iX = Mathf.RoundToInt(index_x);
            int iY = Mathf.RoundToInt(index_y);
            int iZ = Mathf.RoundToInt(-index_x-index_y);

            if(iX+iY+iZ != 0)
            {
                float dX = Mathf.Abs(index_x-iX);
                float dY = Mathf.Abs(index_y-iY);
                float dZ = Mathf.Abs(-(index_x+index_y-iZ));

                if(dX>dY && dX>dZ)
                {
                    iX = -(iY + iZ);
                }
                else if(dZ>dY)
                {
                    iZ = -(iX + iY);
                }
            }
            
            return new Vector3Int(iX,iY,iZ);
        }

        /// <summary>
        /// 六边形索引转局部坐标
        /// </summary>
        /// <param name="hexIndex"></param>
        /// <param name="outRadius"></param>
        /// <returns></returns>
        public static Vector2 HexIndexLocalPosition(Vector3Int hexIndex,float outRadius)
        {
            float innRadius = outRadius*HEX_RADIUS_OUT2INN;

            float space_x = innRadius*2;
            float space_y = outRadius*1.5f;

            float py = hexIndex.z*space_y;
            float px = hexIndex.z*innRadius + hexIndex.x*space_x;

            return new Vector2(px,py);
        }

        /// <summary>
        /// 生成蜂窝结构
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="hexIndexsCache"></param>
        public static void GenerateCellularHexIndex(int radius,List<Vector3Int> hexIndexsCache)
        {
            //从内层向外层绕圈生成

            hexIndexsCache.Clear();

            //第一层 直接加进去
            hexIndexsCache.Add(new Vector3Int(0,0,0));

            //从第二层开始绕圈 
            //有6个斜边 
            //第r层 每个斜边跟着起点延斜边方向添加r-1个节点 最后一个斜边要闭环所以添加r-2个点
            for (int r = 2; r <= radius; r++)
            {
                //从左上顺时针生成
                Vector3Int idx = default;
                idx.x = 0;
                idx.z = r-1;
                idx.y = -idx.z;

                hexIndexsCache.Add(idx);
                
                for (int i = 1; i < r; i++)
                {
                    idx.x += 1;
                    idx.z -= 1;
                    hexIndexsCache.Add(idx);
                }
                for (int i = 1; i < r; i++)
                {
                    idx.z -= 1;
                    idx.y += 1;
                    hexIndexsCache.Add(idx);
                }
                for (int i = 1; i < r; i++)
                {
                    idx.x -= 1;
                    idx.y += 1;
                    hexIndexsCache.Add(idx);
                }
                for (int i = 1; i < r; i++)
                {
                    idx.x -= 1;
                    idx.z += 1;
                    hexIndexsCache.Add(idx);
                }
                for (int i = 1; i < r; i++)
                {
                    idx.z += 1;
                    idx.y -= 1;
                    hexIndexsCache.Add(idx);
                }
                for (int i = 2; i < r; i++)
                {
                    idx.x += 1;
                    idx.y -= 1;
                    hexIndexsCache.Add(idx);
                }

            }

        }

        /// <summary>
        /// 获取周围六个邻居坐标
        /// </summary>
        /// <param name="center"></param>
        /// <param name="idxCache"></param>
        public static void GetHexNear(Vector3Int center,Vector3Int[] idxCache)
        {
            if(idxCache?.Length<6)return;

            for (int i = 0; i < 6; i++)
            {
                idxCache[i] = GetHexSideIndex(center,i);
            }
        }

        //从左上顺时针
        public static Vector3Int GetHexSideIndex(Vector3Int center,int index)
        {
            switch (index)
            {
                case 0: return center + new Vector3Int(0,-1,1);
                case 1: return center + new Vector3Int(1,-1,0);
                case 2: return center + new Vector3Int(1,0,-1);
                case 3: return center + new Vector3Int(0,1,-1);
                case 4: return center + new Vector3Int(-1,1,0);
                case 5: return center + new Vector3Int(-1,0,1);
            }
            return center;
        }

#endregion
    
    }
}

/*
1:1
2:7
3:19
4:37
*/
