using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW
{
    /// <summary>
    /// 无需设定Order 只需设定Layer
    /// 同级根据弹出时间 后出现的更优先 Order自动设定
    /// 强覆盖的设为不同Layer
    /// </summary>
    public static class UICanvasSortingUtility
    {
        private static Dictionary<string,List<Canvas>> layerCanvasDict = new();


        public static void RegisterSort(this Canvas canvas)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError($"UICanvasSortingUtility not sort ScreenSpaceOverlay");
            }
            else
            {
                registerSortCamera(canvas);
            }

        }

        public static void UnregisterSort(this Canvas canvas)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError($"UICanvasSortingUtility not sort ScreenSpaceOverlay");
            }
            else
            {
                unregisterSortCamera(canvas);
            }
        }

        private static void unregisterSortCamera(Canvas canvas)
        {
            var layer = canvas.sortingLayerName;
            var canvasList = layerCanvasDict.GetOrNew(layer);
            canvasList.Remove(canvas);
            resetCanvasListOrder(canvasList);
        }

        private static void registerSortCamera(Canvas canvas)
        {
            var layer = canvas.sortingLayerName;
            var canvasList = layerCanvasDict.GetOrNew(layer);
            canvasList.Remove(canvas);
            canvasList.Add(canvas);
            resetCanvasListOrder(canvasList);
        }

        private static void resetCanvasListOrder(List<Canvas> canvasList)
        {
            for (int i = 0; i < canvasList.Count; i++)
            {
                canvasList[i].sortingOrder = i;
            }
        }
    }
}
