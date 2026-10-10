using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Graph/Start", "Start")]
    public sealed class StartNodeDefinition : NodeDefinition
    {
        public const string StartPortId = "start";
        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(StartPortId, "Start", NodePortDirection.Output, typeof(FlowPort))
        };
        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new StartNodeRuntime(this, executor);
        }
    }
}
