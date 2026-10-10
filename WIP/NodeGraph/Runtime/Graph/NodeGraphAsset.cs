using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class NodeGraphAsset : ScriptableObject
    {
        [SerializeField]
        private int schemaVersion = 1;
        [SerializeField]
        private List<NodeDefinition> nodes = new List<NodeDefinition>();
        [SerializeField]
        private List<NodeEdge> edges = new List<NodeEdge>();
        public int SchemaVersion => schemaVersion;
        public IReadOnlyList<NodeDefinition> Nodes => nodes;
        public IReadOnlyList<NodeEdge> Edges => edges;

        public StartNodeDefinition GetStartNode()
        {
            return nodes.OfType<StartNodeDefinition>().SingleOrDefault();
        }

        public EndNodeDefinition GetEndNode()
        {
            return nodes.OfType<EndNodeDefinition>().SingleOrDefault();
        }

        public bool ContainsNode(NodeDefinition node)
        {
            return node != null && nodes.Contains(node);
        }

        public bool ContainsEdge(NodeEdge edge)
        {
            return edges.Contains(edge);
        }

        internal void AddNode(NodeDefinition node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (nodes.Contains(node))
            {
                throw new InvalidOperationException($"Node '{node.name}' is already part of graph '{name}'.");
            }

            if (node is StartNodeDefinition && GetStartNode() != null)
            {
                throw new InvalidOperationException($"Graph '{name}' already contains a Start node.");
            }

            if (node is EndNodeDefinition && GetEndNode() != null)
            {
                throw new InvalidOperationException($"Graph '{name}' already contains an End node.");
            }

            nodes.Add(node);
        }

        internal void RemoveNode(NodeDefinition node)
        {
            if (node == null)
            {
                return;
            }

            nodes.Remove(node);
            edges.RemoveAll(edge => edge.OutputNode == node || edge.InputNode == node);
        }

        internal bool TryAddEdge(NodeEdge edge, out string error)
        {
            if (!TryValidateEdge(edge, out error))
            {
                return false;
            }

            if (edges.Contains(edge))
            {
                error = "The edge already exists.";
                return false;
            }

            edges.Add(edge);
            error = null;
            return true;
        }

        internal bool RemoveEdge(NodeEdge edge)
        {
            return edges.Remove(edge);
        }

        internal bool TryValidateEdge(NodeEdge edge, out string error)
        {
            if (edge.OutputNode == null || edge.InputNode == null)
            {
                error = "Edge nodes cannot be null.";
                return false;
            }

            if (edge.OutputNode == edge.InputNode)
            {
                error = "A node cannot connect to itself.";
                return false;
            }

            if (!nodes.Contains(edge.OutputNode) || !nodes.Contains(edge.InputNode))
            {
                error = "Both edge nodes must belong to the graph.";
                return false;
            }

            if (!edge.OutputNode.TryGetPort(edge.OutputPortId, out var outputPort) || outputPort.Direction != NodePortDirection.Output)
            {
                error = "The output port does not exist.";
                return false;
            }

            if (!edge.InputNode.TryGetPort(edge.InputPortId, out var inputPort) || inputPort.Direction != NodePortDirection.Input)
            {
                error = "The input port does not exist.";
                return false;
            }

            if (outputPort.ValueType != inputPort.ValueType)
            {
                error = "The port types do not match.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
