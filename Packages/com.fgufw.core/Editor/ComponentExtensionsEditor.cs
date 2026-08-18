using FGUFW;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    public static class ComponentExtensionsEditor
    {
        [MenuItem("CONTEXT/MonoBehaviour/Auto Ref Fields")]
        private static void AutoRefFields(MenuCommand command)
        {
            var component = (MonoBehaviour)command.context;
            Undo.RecordObject(component, "Auto Ref Fields");
            component.AutoRefField();
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            EditorUtility.SetDirty(component);
        }
    }
}
