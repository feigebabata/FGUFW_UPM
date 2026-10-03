using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FGUFW.Editor
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var names = SortingLayer.layers.Select(l => l.name).ToArray();
            if (names.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            int index = Array.IndexOf(names, property.stringValue);
            if (index < 0)
            {
                // 值非法（或为空）时，暂时显示 Default，但不覆盖用户数据
                EditorGUI.showMixedValue = false;
                index = Mathf.Max(0, Array.IndexOf(names, "Default"));
            }

            int newIndex = EditorGUI.Popup(position, label.text, index, names);
            if (newIndex != index || string.IsNullOrEmpty(property.stringValue))
                property.stringValue = names[newIndex];

            EditorGUI.EndProperty();
        }
    }
}