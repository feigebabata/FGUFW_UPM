using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    [CustomPropertyDrawer(typeof(AnyData))]
    public class AnyDataDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            object fieldValue = fieldInfo.GetValue(property.serializedObject.targetObject);
            // if(fieldInfo==default)return;

            var anyData = fieldValue as AnyData;

            var data = anyData.Get<object>();
            
            GUI.enabled = false;
            if(data==default)
            {
                EditorGUI.LabelField(position,"Data : Null");
            }
            else if(data is Component)
            {
                var serializedProperty = property.FindPropertyRelative("Data");
                EditorGUI.PropertyField(position,serializedProperty);
            }
            else if(data is GameObject)
            {
                var serializedProperty = property.FindPropertyRelative("m_Data");
                EditorGUI.PropertyField(position,serializedProperty);
            }
            else
            {
                EditorGUI.LabelField(position,$"Data : {data.ts()}");
            }
            GUI.enabled = true;

        }
    }
}