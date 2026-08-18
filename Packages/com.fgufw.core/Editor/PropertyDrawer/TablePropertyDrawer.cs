// using FGUFW;
// using UnityEditor;
// using UnityEngine;

// [CustomPropertyDrawer(typeof(Table<,>))]
// public class SerializableDictionaryDrawer : PropertyDrawer
// {
//     private const float lineHeight = 20f;
//     private const float padding = 5f;
    
//     // 使用 PropertyDrawer 的 fieldInfo 来存储折叠状态
//     private bool isExpanded = true;
    
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);
        
//         // 计算折叠按钮区域
//         Rect foldoutRect = new Rect(
//             position.x,
//             position.y,
//             EditorGUIUtility.labelWidth,
//             lineHeight
//         );
        
//         // 绘制折叠按钮
//         isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, label, true);
        
//         SerializedProperty keysProp = property.FindPropertyRelative("keys");
//         SerializedProperty valuesProp = property.FindPropertyRelative("values");
        
//         // 如果折叠，只显示条目数量
//         if (!isExpanded)
//         {
//             EditorGUI.LabelField(
//                 new Rect(position.x + EditorGUIUtility.labelWidth + padding, position.y, 
//                        position.width - EditorGUIUtility.labelWidth - padding, lineHeight),
//                 $"({keysProp.arraySize} items)"
//             );
//             EditorGUI.EndProperty();
//             return;
//         }
        
//         // 获取 keys 和 values 数组
        
//         // 计算内容区域
//         Rect contentRect = new Rect(
//             position.x + padding,
//             position.y + lineHeight + padding,
//             position.width - padding * 2,
//             position.height - lineHeight - padding * 2
//         );
        
//         // 开始绘制字典内容
//         EditorGUI.BeginChangeCheck();
        
//         // 绘制键值对
//         float currentY = contentRect.y;
        
//         for (int i = 0; i < keysProp.arraySize; i++)
//         {
//             Rect keyRect = new Rect(
//                 contentRect.x,
//                 currentY,
//                 contentRect.width * 0.45f,
//                 lineHeight
//             );
            
//             Rect valueRect = new Rect(
//                 contentRect.x + contentRect.width * 0.5f,
//                 currentY,
//                 contentRect.width * 0.45f,
//                 lineHeight
//             );
            
//             Rect removeButtonRect = new Rect(
//                 contentRect.x + contentRect.width * 0.95f,
//                 currentY,
//                 20,
//                 lineHeight
//             );
            
//             // 绘制键
//             EditorGUI.PropertyField(keyRect, keysProp.GetArrayElementAtIndex(i), GUIContent.none);
            
//             // 绘制值
//             EditorGUI.PropertyField(valueRect, valuesProp.GetArrayElementAtIndex(i), GUIContent.none);
            
//             // 删除按钮
//             if (GUI.Button(removeButtonRect, "×"))
//             {
//                 keysProp.DeleteArrayElementAtIndex(i);
//                 valuesProp.DeleteArrayElementAtIndex(i);
//                 break; // 退出循环，避免索引越界
//             }
            
//             currentY += lineHeight + padding;
//         }
        
//         // 添加新条目按钮
//         Rect addButtonRect = new Rect(
//             contentRect.x,
//             currentY,
//             contentRect.width,
//             lineHeight
//         );
        
//         if (GUI.Button(addButtonRect, "Add New Entry"))
//         {
//             keysProp.arraySize++;
//             valuesProp.arraySize++;
//         }
        
//         if (EditorGUI.EndChangeCheck())
//         {
//             property.serializedObject.ApplyModifiedProperties();
//         }
        
//         EditorGUI.EndProperty();
//     }
    
//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         if (!isExpanded)
//         {
//             // 折叠状态只显示一行
//             return lineHeight;
//         }
        
//         SerializedProperty keysProp = property.FindPropertyRelative("keys");
//         int itemCount = keysProp.arraySize;
        
//         // 计算总高度：
//         // 1. 标题行
//         // 2. 每个键值对
//         // 3. 添加按钮
//         // 4. 所有间距
//         return lineHeight + // 标题
//                (lineHeight + padding) * (itemCount + 1) + // 条目 + 添加按钮
//                padding * 2; // 上下边距
//     }
// }