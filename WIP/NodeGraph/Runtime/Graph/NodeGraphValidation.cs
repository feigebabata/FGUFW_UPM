using System;
using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public sealed class NodeGraphValidationResult
    {
        private readonly List<string> errors = new List<string>();
        public bool IsValid => errors.Count == 0;
        public IReadOnlyList<string> Errors => errors;
        public string ErrorMessage => string.Join(Environment.NewLine, errors);

        internal void Add(string error)
        {
            errors.Add(error);
        }
    }

    public static class NodeGraphValidator
    {
        public static NodeGraphValidationResult Validate(NodeGraphAsset graph)
        {
            var result = new NodeGraphValidationResult();
            if (graph == null)
            {
                result.Add("Graph asset is null.");
                return result;
            }

            var nodes = new HashSet<NodeDefinition>();
            var startCount = 0;
            var endCount = 0;
            for (var i = 0; i < graph.Nodes.Count; i++)
            {
                var node = graph.Nodes[i];
                if (node == null)
                {
                    result.Add($"Node at index {i} is missing.");
                    continue;
                }

                if (!nodes.Add(node))
                {
                    result.Add($"Node '{node.name}' is listed more than once.");
                    continue;
                }

                if (node is StartNodeDefinition)
                {
                    startCount++;
                }
                else if (node is EndNodeDefinition)
                {
                    endCount++;
                }

                ValidatePorts(node, result);
            }

            if (startCount != 1)
            {
                result.Add($"Graph must contain exactly one Start node, found {startCount}.");
            }

            if (endCount != 1)
            {
                result.Add($"Graph must contain exactly one End node, found {endCount}.");
            }

            var uniqueEdges = new HashSet<NodeEdge>();
            for (var i = 0; i < graph.Edges.Count; i++)
            {
                var edge = graph.Edges[i];
                if (!uniqueEdges.Add(edge))
                {
                    result.Add($"Edge at index {i} is duplicated.");
                }

                ValidateEdge(graph, edge, i, result);
            }

            return result;
        }

        private static void ValidatePorts(NodeDefinition node, NodeGraphValidationResult result)
        {
            var ports = node.Ports;
            if (ports == null)
            {
                result.Add($"Node '{node.name}' returned a null port list.");
                return;
            }

            var portIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < ports.Count; i++)
            {
                var port = ports[i];
                if (string.IsNullOrWhiteSpace(port.Id))
                {
                    result.Add($"Node '{node.name}' has an empty port id.");
                }
                else if (!portIds.Add(port.Id))
                {
                    result.Add($"Node '{node.name}' has duplicate port id '{port.Id}'.");
                }

                if (port.ValueType == null)
                {
                    result.Add($"Port '{port.Id}' on node '{node.name}' has no value type.");
                }
            }

            if (node is StartNodeDefinition
                && (ports.Count != 1
                    || ports[0].Direction != NodePortDirection.Output
                    || ports[0].ValueType != typeof(FlowPort)))
            {
                result.Add("Start must have one Flow output port.");
            }

            if (node is EndNodeDefinition
                && (ports.Count != 1
                    || ports[0].Direction != NodePortDirection.Input
                    || ports[0].ValueType != typeof(FlowPort)))
            {
                result.Add("End must have one Flow input port.");
            }
        }

        private static void ValidateEdge(NodeGraphAsset graph, NodeEdge edge, int index, NodeGraphValidationResult result)
        {
            if (!graph.TryValidateEdge(edge, out var error))
            {
                result.Add($"Edge {index}: {error}");
            }
        }
    }
}
