using System.IO;
using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    internal static class NodeGraphAssetFactory
    {
        [MenuItem("Assets/Create/FGUFW/Node Graph", false, 201)]
        private static void CreateNodeGraph()
        {
            var folder = GetSelectedFolder();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/NodeGraph.asset");
            var graph = CreateAtPath(path);
            ProjectWindowUtil.ShowCreatedAsset(graph);
            NodeGraphEditorWindow.Open(graph);
        }

        internal static NodeGraphAsset CreateAtPath(string path)
        {
            var graph = ScriptableObject.CreateInstance<NodeGraphAsset>();
            graph.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(graph, path);
            try
            {
                NodeGraphEditorUtility.CreateRequiredNode<StartNodeDefinition>(graph, new Vector2(-220f, 0f));
                NodeGraphEditorUtility.CreateRequiredNode<EndNodeDefinition>(graph, new Vector2(220f, 0f));
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                return graph;
            }
            catch
            {
                AssetDatabase.DeleteAsset(path);
                throw;
            }
        }

        private static string GetSelectedFolder()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(path))
            {
                return "Assets";
            }

            if (!AssetDatabase.IsValidFolder(path))
            {
                path = Path.GetDirectoryName(path)?.Replace('\\', '/');
            }

            return string.IsNullOrWhiteSpace(path) ? "Assets" : path;
        }
    }
}
