using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Graph/End", "End")]
    public sealed class EndNodeDefinition : NodeDefinition
    {
        public const string EndPortId = "end";
        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(EndPortId, "End", NodePortDirection.Input, typeof(FlowPort))
        };
        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new EndNodeRuntime(this, executor);
        }
    }
}
