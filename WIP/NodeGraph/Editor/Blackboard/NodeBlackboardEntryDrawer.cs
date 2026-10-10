using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    [CustomPropertyDrawer(typeof(NodeBlackboardEntry))]
    internal sealed class NodeBlackboardEntryDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyProperty = property.FindPropertyRelative("key");
            var typeProperty = property.FindPropertyRelative("type");
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var firstLine = new Rect(position.x, position.y, position.width, lineHeight);
            var secondLine = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
            var keyRect = new Rect(firstLine.x, firstLine.y, firstLine.width * 0.6f - 2f, lineHeight);
            var typeRect = new Rect(keyRect.xMax + 4f, firstLine.y, firstLine.width * 0.4f - 2f, lineHeight);

            EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
            EditorGUI.PropertyField(secondLine, GetValueProperty(property, typeProperty), new GUIContent("Value"));

            EditorGUI.EndProperty();
        }

        private static SerializedProperty GetValueProperty(SerializedProperty property, SerializedProperty typeProperty)
        {
            switch ((NodeBlackboardValueType)typeProperty.enumValueIndex)
            {
                case NodeBlackboardValueType.Bool:
                    return property.FindPropertyRelative("boolValue");
                case NodeBlackboardValueType.Int:
                    return property.FindPropertyRelative("intValue");
                case NodeBlackboardValueType.Float:
                    return property.FindPropertyRelative("floatValue");
                case NodeBlackboardValueType.String:
                    return property.FindPropertyRelative("stringValue");
                case NodeBlackboardValueType.Vector2:
                    return property.FindPropertyRelative("vector2Value");
                case NodeBlackboardValueType.Vector3:
                    return property.FindPropertyRelative("vector3Value");
                case NodeBlackboardValueType.Color:
                    return property.FindPropertyRelative("colorValue");
                case NodeBlackboardValueType.Object:
                    return property.FindPropertyRelative("objectValue");
                default:
                    return property.FindPropertyRelative("stringValue");
            }
        }
    }
}
