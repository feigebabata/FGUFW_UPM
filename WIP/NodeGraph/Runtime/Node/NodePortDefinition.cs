using System;

namespace FGUFW.NodeGraph
{
    public readonly struct NodePortDefinition : IEquatable<NodePortDefinition>
    {
        public NodePortDefinition(string id, string displayName, NodePortDirection direction, Type valueType)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Port id cannot be empty.", nameof(id));
            }

            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName;
            Direction = direction;
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        public string Id { get; }
        public string DisplayName { get; }
        public NodePortDirection Direction { get; }
        public Type ValueType { get; }

        public bool Equals(NodePortDefinition other)
        {
            return string.Equals(Id, other.Id, StringComparison.Ordinal) && Direction == other.Direction && ValueType == other.ValueType;
        }

        public override bool Equals(object obj)
        {
            return obj is NodePortDefinition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(Id);
                hash = (hash * 397) ^ (int)Direction;
                hash = (hash * 397) ^ ValueType.GetHashCode();
                return hash;
            }
        }
    }
}
