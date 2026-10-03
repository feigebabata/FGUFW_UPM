using UnityEngine;

namespace FGUFW
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIBase : MonoBehaviour
    {
        public Canvas UICanvas;
        public CanvasGroup Group;

        [SortingLayer]
        public string SortingLayer;

        void OnValidate()
        {
            UICanvas = GetComponent<Canvas>();
            Group = GetComponent<CanvasGroup>();
        }

        public virtual void OnCreate()
        {
            gameObject.SetActive(false);
        }

        public virtual void OnOpen()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnClose()
        {
            gameObject.SetActive(false);
        }

    }
    
}
