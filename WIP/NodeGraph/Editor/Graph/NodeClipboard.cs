using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    internal static class NodeClipboard
    {
        private const string Header = "FGUFW_NODE_GRAPH_V2\n";
        private const int CurrentVersion = 2;
        public static string Serialize(
            NodeGraphAsset graph,
            IEnumerable<NodeDefinition> selectedNodes,
            IEnumerable<NodeEdge> explicitlySelectedEdges = null)
        {
            if (graph == null || selectedNodes == null)
            {
                return string.Empty;
            }

            var nodes = selectedNodes
                .Where(node => node != null
                    && graph.ContainsNode(node)
                    && !(node is StartNodeDefinition)
                    && !(node is EndNodeDefinition))
                .Distinct()
                .ToArray();
            if (nodes.Length == 0)
            {
                return string.Empty;
            }

            var center = Vector2.zero;
            for (var i = 0; i < nodes.Length; i++)
            {
                center += nodes[i].Position;
            }

            center /= nodes.Length;
            var payload = new GraphClipboardData
            {
                Version = CurrentVersion,
                SourceCenter = center
            };
            var localIds = new Dictionary<NodeDefinition, int>();
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                localIds.Add(node, i);
                payload.Nodes.Add(new ClipboardNode
                {
                    LocalId = i,
                    TypeName = node.GetType().AssemblyQualifiedName,
                    Json = EditorJsonUtility.ToJson(node),
                    RelativePosition = node.Position - center
                });
            }

            var edgesToCopy = new HashSet<NodeEdge>();
            for (var i = 0; i < graph.Edges.Count; i++)
            {
                var edge = graph.Edges[i];
                if (localIds.ContainsKey(edge.OutputNode) && localIds.ContainsKey(edge.InputNode))
                {
                    edgesToCopy.Add(edge);
                }
            }

            if (explicitlySelectedEdges != null)
            {
                foreach (var edge in explicitlySelectedEdges)
                {
                    if (localIds.ContainsKey(edge.OutputNode) || localIds.ContainsKey(edge.InputNode))
                    {
                        edgesToCopy.Add(edge);
                    }
                }
            }

            foreach (var edge in edgesToCopy)
            {
                payload.Edges.Add(CreateClipboardEdge(edge, localIds));
            }

            return Header + JsonUtility.ToJson(payload);
        }

        public static bool CanPaste(string serializedData)
        {
            return TryParse(serializedData, out _, out _);
        }

        public static bool TryGetSourceCenter(string serializedData, out Vector2 sourceCenter)
        {
            if (TryParse(serializedData, out var payload, out _))
            {
                sourceCenter = payload.SourceCenter;
                return true;
            }

            sourceCenter = default;
            return false;
        }

        public static bool TryPaste(
            NodeGraphAsset graph,
            string serializedData,
            Vector2 anchor,
            out List<NodeDefinition> pastedNodes,
            out string error)
        {
            pastedNodes = new List<NodeDefinition>();
            if (graph == null)
            {
                error = "No target graph is selected.";
                return false;
            }

            if (!TryParse(serializedData, out var payload, out error))
            {
                return false;
            }

            var resolvedTypes = new Dictionary<int, Type>();
            for (var i = 0; i < payload.Nodes.Count; i++)
            {
                var record = payload.Nodes[i];
                var type = Type.GetType(record.TypeName);
                if (type == null
                    || type.IsAbstract
                    || !typeof(NodeDefinition).IsAssignableFrom(type)
                    || type == typeof(StartNodeDefinition)
                    || type == typeof(EndNodeDefinition))
                {
                    error = $"Node type '{record.TypeName}' is unavailable.";
                    return false;
                }

                if (!resolvedTypes.TryAdd(record.LocalId, type))
                {
                    error = $"Clipboard node id '{record.LocalId}' is duplicated.";
                    return false;
                }
            }

            var nodeMap = new Dictionary<int, NodeDefinition>();
            try
            {
                for (var i = 0; i < payload.Nodes.Count; i++)
                {
                    var record = payload.Nodes[i];
                    var node = NodeGraphEditorUtility.CreateNode(graph, resolvedTypes[record.LocalId], anchor + record.RelativePosition);
                    EditorJsonUtility.FromJsonOverwrite(record.Json, node);
                    node.SetPosition(anchor + record.RelativePosition);
                    node.name = NodeTypeRegistry.GetInfo(node.GetType()).DisplayName;
                    nodeMap.Add(record.LocalId, node);
                    pastedNodes.Add(node);
                    EditorUtility.SetDirty(node);
                }

                for (var i = 0; i < payload.Edges.Count; i++)
                {
                    var record = payload.Edges[i];
                    var outputNode = ResolveNode(graph, nodeMap, record.OutputNodeId, record.OutputExternalId);
                    var inputNode = ResolveNode(graph, nodeMap, record.InputNodeId, record.InputExternalId);
                    if (outputNode == null || inputNode == null)
                    {
                        // External endpoints only exist when pasting back into
                        // the source graph. Cross-graph paste skips them.
                        continue;
                    }

                    var edge = new NodeEdge(outputNode, record.OutputPortId, inputNode, record.InputPortId);
                    if (!graph.TryAddEdge(edge, out var edgeError))
                    {
                        throw new InvalidOperationException(edgeError);
                    }
                }

                EditorUtility.SetDirty(graph);
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                for (var i = pastedNodes.Count - 1; i >= 0; i--)
                {
                    var node = pastedNodes[i];
                    graph.RemoveNode(node);
                    UnityEngine.Object.DestroyImmediate(node, true);
                }

                pastedNodes.Clear();
                EditorUtility.SetDirty(graph);
                error = exception.Message;
                return false;
            }
        }

        private static ClipboardEdge CreateClipboardEdge(NodeEdge edge, IReadOnlyDictionary<NodeDefinition, int> localIds)
        {
            var result = new ClipboardEdge
            {
                OutputNodeId = -1,
                OutputPortId = edge.OutputPortId,
                InputNodeId = -1,
                InputPortId = edge.InputPortId
            };
            if (localIds.TryGetValue(edge.OutputNode, out var outputId))
            {
                result.OutputNodeId = outputId;
            }
            else
            {
                result.OutputExternalId = GlobalObjectId.GetGlobalObjectIdSlow(edge.OutputNode).ToString();
            }

            if (localIds.TryGetValue(edge.InputNode, out var inputId))
            {
                result.InputNodeId = inputId;
            }
            else
            {
                result.InputExternalId = GlobalObjectId.GetGlobalObjectIdSlow(edge.InputNode).ToString();
            }

            return result;
        }

        private static NodeDefinition ResolveNode(
            NodeGraphAsset graph,
            IReadOnlyDictionary<int, NodeDefinition> localNodes,
            int localId,
            string externalId)
        {
            if (localId >= 0)
            {
                if (!localNodes.TryGetValue(localId, out var localNode))
                {
                    throw new InvalidOperationException($"Clipboard edge references missing node id '{localId}'.");
                }

                return localNode;
            }

            if (string.IsNullOrWhiteSpace(externalId) || !GlobalObjectId.TryParse(externalId, out var objectId))
            {
                return null;
            }

            var externalNode = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(objectId) as NodeDefinition;
            return graph.ContainsNode(externalNode) ? externalNode : null;
        }

        private static bool TryParse(string serializedData, out GraphClipboardData payload, out string error)
        {
            payload = null;
            if (string.IsNullOrEmpty(serializedData) || !serializedData.StartsWith(Header, StringComparison.Ordinal))
            {
                error = "Clipboard data is not a NodeGraph payload.";
                return false;
            }

            try
            {
                payload = JsonUtility.FromJson<GraphClipboardData>(serializedData.Substring(Header.Length));
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }

            if (payload == null
                || payload.Version != CurrentVersion
                || payload.Nodes == null
                || payload.Nodes.Count == 0
                || payload.Edges == null)
            {
                error = "Clipboard data is invalid or unsupported.";
                return false;
            }

            error = null;
            return true;
        }

        [Serializable]
        private sealed class GraphClipboardData
        {
            public int Version;
            public Vector2 SourceCenter;
            public List<ClipboardNode> Nodes = new List<ClipboardNode>();
            public List<ClipboardEdge> Edges = new List<ClipboardEdge>();
        }

        [Serializable]
        private sealed class ClipboardNode
        {
            public int LocalId;
            public string TypeName;
            public string Json;
            public Vector2 RelativePosition;
        }

        [Serializable]
        private sealed class ClipboardEdge
        {
            public int OutputNodeId = -1;
            public string OutputExternalId;
            public string OutputPortId;
            public int InputNodeId = -1;
            public string InputExternalId;
            public string InputPortId;
        }
    }
}
