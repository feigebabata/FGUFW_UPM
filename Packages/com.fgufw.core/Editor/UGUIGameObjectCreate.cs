using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FGUFW.Editor
{
    static class UGUIGameObjectCreate
    {
        [MenuItem("GameObject/UI/ImgBtn", validate = true)]
        static bool checkCreateBtn()
        {
            return Selection.activeGameObject != default;
        }

        [MenuItem("GameObject/UI/ImgBtn")]
        static void createBtn()
        {
            var parent = Selection.activeGameObject.transform;

            var go = new GameObject("ImgBtn");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = default;
            go.AddComponent<Image>();
            go.AddComponent<Button>();

            Selection.activeGameObject = go;
        }



        [MenuItem("GameObject/UI/ScrollList", validate = true)]
        static bool checkCreatesollder()
        {
            return Selection.activeGameObject != default;
        }

        [MenuItem("GameObject/UI/ScrollList")]
        static void createsollder()
        {
            var parent = Selection.activeGameObject.transform;

            var go = new GameObject("ScrollList");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = default;
            go.AddComponent<Image>();
            var scroll = go.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            go.AddComponent<RectMask2D>();

            var listGO = new GameObject("List");
            listGO.transform.SetParent(go.transform, false);
            var layoutGroup = listGO.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;

            var contentSize = listGO.AddComponent<ContentSizeFitter>();
            contentSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rt = listGO.transform as RectTransform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);

            rt.offsetMin = default;
            rt.offsetMax = default;

            rt.pivot = new Vector2(0.5f, 1);

            scroll.content = rt;

            Selection.activeGameObject = go;

        }


        [MenuItem("GameObject/UI/Text_TMP", validate = true)]
        static bool checkCreateTMPText()
        {
            return Selection.activeGameObject != default;
        }


        [MenuItem("GameObject/UI/Text", validate = true)]
        static bool checkCreateText()
        {
            return Selection.activeGameObject != default;
        }

        [MenuItem("GameObject/UI/Text")]
        static void createText()
        {
            var parent = Selection.activeGameObject.transform;

            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = default;
            var text = go.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;

            Selection.activeGameObject = go;
        }
        
    }

}
