using System.Reflection;
using UnityEditor;

namespace FGUFW.Editor
{
    public static class EditorConsole
    {
        public static void Clear()
        {
            // 获取 UnityEditorInternal.LogEntries 类型
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            
            // 获取 Clear 方法并调用
            var method = type.GetMethod("Clear");
            method?.Invoke(null, null);
        }
    }
}