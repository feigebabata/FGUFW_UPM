using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    [CustomPropertyDrawer(typeof(NodeValue), true)]
    internal sealed class NodeValueDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var modeProperty = property.FindPropertyRelative("mode");
            var usesBlackboard = (NodeValueMode)modeProperty.enumValueIndex == NodeValueMode.Blackboard;
            var labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, position.width * 0.5f);
            var toggleRect = new Rect(position.x, position.y, 18f, position.height);
            var labelRect = new Rect(toggleRect.xMax, position.y, labelWidth - toggleRect.width, position.height);
            var valueRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.BeginChangeCheck();
            usesBlackboard = EditorGUI.Toggle(toggleRect, new GUIContent(string.Empty, "Use Blackboard"), usesBlackboard);
            if (EditorGUI.EndChangeCheck())
            {
                modeProperty.enumValueIndex = usesBlackboard ? (int)NodeValueMode.Blackboard : (int)NodeValueMode.Constant;
            }

            EditorGUI.LabelField(labelRect, usesBlackboard ? "Blackboard" : label.text);
            var valueProperty = property.FindPropertyRelative(usesBlackboard ? "blackboardKey" : "constantValue");
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}
