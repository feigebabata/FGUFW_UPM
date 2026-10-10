using UnityEditor;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    [CustomEditor(typeof(NodeGraphAsset))]
    internal sealed class NodeGraphAssetInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var openButton = new Button(() => NodeGraphEditorWindow.Open((NodeGraphAsset)target))
            {
                text = "Open Node Graph"
            };
            root.Add(openButton);
            var validation = NodeGraphEditorUtility.Validate((NodeGraphAsset)target);
            var message = validation.IsValid ? "Graph is valid." : validation.ErrorMessage;
            var messageType = validation.IsValid ? HelpBoxMessageType.Info : HelpBoxMessageType.Error;
            var status = new HelpBox(message, messageType);
            root.Add(status);
            return root;
        }
    }
}
