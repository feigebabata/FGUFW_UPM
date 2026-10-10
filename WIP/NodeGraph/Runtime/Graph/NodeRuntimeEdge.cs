namespace FGUFW.NodeGraph
{
    public sealed class NodeRuntimeEdge
    {
        public NodeRuntimeEdge(NodeRuntime outputNode, string outputPortId, NodeRuntime inputNode, string inputPortId)
        {
            OutputNode = outputNode;
            OutputPortId = outputPortId;
            InputNode = inputNode;
            InputPortId = inputPortId;
        }

        public NodeRuntime OutputNode { get; }
        public string OutputPortId { get; }
        public NodeRuntime InputNode { get; }
        public string InputPortId { get; }
    }
}
