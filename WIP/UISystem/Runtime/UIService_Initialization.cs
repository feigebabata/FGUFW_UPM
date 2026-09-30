using FGUFW;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace FGUFW.UISystem
{
    public partial class UIService : MonoSingleton<UIService>
    {
        private const string UIRootName = "UIRoot";
        private const string UICameraName = "UICamera";

        public Transform UIRoot {get;private set;}
        public Camera UICamera {get;private set;}
        public EventSystem UIEventSystem {get;private set;}
        
        protected override bool IsDontDestroyOnLoad()=>true;
        protected override void Init()
        {
            createUIRoot();
            createUICamera();
            findOrCreateEventSystem();
            SceneManager.sceneLoaded += onSceneLoaded;
        }

        public override void Dispose()
        {
            SceneManager.sceneLoaded -= onSceneLoaded;
            base.Dispose();
        }

        private void createUIRoot()
        {
            UIRoot = transform.Find(UIRootName);
            if(UIRoot)return;

            var uiRootObject = new GameObject(UIRootName,typeof(RectTransform));
            UIRoot = uiRootObject.transform;
            UIRoot.SetParent(transform,false);
        }

        private void createUICamera()
        {
            var uiCameraTransform = transform.Find(UICameraName);
            if(!uiCameraTransform)
            {
                var uiCameraObject = new GameObject(UICameraName);
                uiCameraTransform = uiCameraObject.transform;
                uiCameraTransform.SetParent(transform,false);
            }

            UICamera = uiCameraTransform.GetComponent<Camera>();
            if(!UICamera)
            {
                UICamera = uiCameraTransform.gameObject.AddComponent<Camera>();
            }

            UICamera.clearFlags = CameraClearFlags.Depth;
            UICamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            UICamera.orthographic = true;
            UICamera.useOcclusionCulling = false;
        }

        private void findOrCreateEventSystem()
        {
            UIEventSystem = FindFirstObjectByType<EventSystem>();
            if(UIEventSystem)return;

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(transform,false);
            UIEventSystem = eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void onSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
        {
            findOrCreateEventSystem();
        }

        private void setupUICanvas(UIBase uiBase)
        {
            uiBase.UICanvas = uiBase.GetComponent<Canvas>();
            uiBase.Group = uiBase.GetComponent<CanvasGroup>();
            uiBase.UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            uiBase.UICanvas.worldCamera = UICamera;
        }
    }
}
