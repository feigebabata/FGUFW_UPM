using UnityEngine;
using UnityEngine.UI;

namespace FGUFW
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        private RectTransform _rectTransform;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void Reset()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        void ApplySafeArea()
        {
            // 获取屏幕的安全区域
            Rect safeArea = Screen.safeArea;

            // 转换为屏幕宽高比例
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            Vector2 anchorMin = safeArea.position / screenSize;
            Vector2 anchorMax = (safeArea.position + safeArea.size) / screenSize;

            // 设置 RectTransform 的锚点
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // 重置偏移
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            // Debug.Log($"Safe Area Applied: AnchorMin {anchorMin}, AnchorMax {anchorMax}");
        }
    }
}
