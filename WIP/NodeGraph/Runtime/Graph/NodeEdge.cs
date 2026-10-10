using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public struct NodeEdge : IEquatable<NodeEdge>
    {
        public NodeEdge(NodeDefinition outputNode, string outputPortId, NodeDefinition inputNode, string inputPortId)
        {
            this.outputNode = outputNode;
            this.outputPortId = outputPortId;
            this.inputNode = inputNode;
            this.inputPortId = inputPortId;
        }

        [SerializeField]
        private NodeDefinition outputNode;
        [SerializeField]
        private string outputPortId;
        [SerializeField]
        private NodeDefinition inputNode;
        [SerializeField]
        private string inputPortId;
        public NodeDefinition OutputNode => outputNode;
        public string OutputPortId => outputPortId;
        public NodeDefinition InputNode => inputNode;
        public string InputPortId => inputPortId;

        public bool Equals(NodeEdge other)
        {
            return outputNode == other.outputNode
                && string.Equals(outputPortId, other.outputPortId, StringComparison.Ordinal)
                && inputNode == other.inputNode
                && string.Equals(inputPortId, other.inputPortId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is NodeEdge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = outputNode != null ? outputNode.GetHashCode() : 0;
                hash = (hash * 397) ^ (outputPortId != null ? StringComparer.Ordinal.GetHashCode(outputPortId) : 0);
                hash = (hash * 397) ^ (inputNode != null ? inputNode.GetHashCode() : 0);
                hash = (hash * 397) ^ (inputPortId != null ? StringComparer.Ordinal.GetHashCode(inputPortId) : 0);
                return hash;
            }
        }
    }
}
