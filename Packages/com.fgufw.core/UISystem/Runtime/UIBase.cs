using UnityEngine;

namespace FGUFW.UISystem
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIBase : MonoBehaviour
    {
        public Canvas UICanvas;
        public CanvasGroup Group;

        void OnValidate()
        {
            UICanvas = GetComponent<Canvas>();
            Group = GetComponent<CanvasGroup>();
        }

        public virtual void OnCreate()
        {
            gameObject.SetActive(false);
        }

        public abstract void OnOpen();

        public abstract void OnClose();

    }
    
}
