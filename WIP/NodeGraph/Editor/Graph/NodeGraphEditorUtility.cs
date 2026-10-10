using System;
using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    internal static class NodeGraphEditorUtility
    {
        public static NodeDefinition CreateNode(NodeGraphAsset graph, Type nodeType, Vector2 position)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            if (nodeType == null || nodeType.IsAbstract || !typeof(NodeDefinition).IsAssignableFrom(nodeType))
            {
                throw new ArgumentException("Node type must be a concrete NodeDefinition.", nameof(nodeType));
            }

            if (nodeType == typeof(StartNodeDefinition) || nodeType == typeof(EndNodeDefinition))
            {
                throw new InvalidOperationException("Start and End can only be created by the graph factory.");
            }

            return CreateNodeInternal(graph, nodeType, position);
        }

        public static T CreateRequiredNode<T>(NodeGraphAsset graph, Vector2 position)
            where T : NodeDefinition
        {
            return (T)CreateNodeInternal(graph, typeof(T), position);
        }

        public static void DeleteNode(NodeGraphAsset graph, NodeDefinition node)
        {
            if (graph == null || node == null)
            {
                return;
            }

            if (node is StartNodeDefinition || node is EndNodeDefinition)
            {
                return;
            }

            graph.RemoveNode(node);
            EditorUtility.SetDirty(graph);
            UnityEngine.Object.DestroyImmediate(node, true);
        }

        public static NodeGraphValidationResult Validate(NodeGraphAsset graph)
        {
            var result = NodeGraphValidator.Validate(graph);
            if (graph == null)
            {
                return result;
            }

            var graphPath = AssetDatabase.GetAssetPath(graph);
            for (var i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                if (node == null)
                {
                    continue;
                }

                if (!string.Equals(AssetDatabase.GetAssetPath(node), graphPath, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add($"Node '{node.name}' is not a SubAsset of graph '{graph.name}'.");
                }
            }

            return result;
        }

        private static NodeDefinition CreateNodeInternal(NodeGraphAsset graph, Type nodeType, Vector2 position)
        {
            if (!AssetDatabase.Contains(graph))
            {
                throw new InvalidOperationException("The graph must be saved as an asset before adding nodes.");
            }

            var node = ScriptableObject.CreateInstance(nodeType) as NodeDefinition;
            if (node == null)
            {
                throw new InvalidOperationException($"Could not create node type '{nodeType.FullName}'.");
            }

            var info = NodeTypeRegistry.GetInfo(nodeType);
            node.name = info.DisplayName;
            node.hideFlags = HideFlags.HideInHierarchy;
            node.SetPosition(position);
            try
            {
                AssetDatabase.AddObjectToAsset(node, graph);
                graph.AddNode(node);
                EditorUtility.SetDirty(node);
                EditorUtility.SetDirty(graph);
                return node;
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(node, true);
                throw;
            }
        }
    }
}
