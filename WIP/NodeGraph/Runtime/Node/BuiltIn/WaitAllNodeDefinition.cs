using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Flow/Wait All", "Wait All")]
    public sealed class WaitAllNodeDefinition : ProgressNodeDefinition
    {
        public const string InputPortId = "wait";
        public const string CompletePortId = "complete";
        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Wait", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(CompletePortId, "Complete", NodePortDirection.Output, typeof(FlowPort))
        };
        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new WaitAllNodeRuntime(this, executor);
        }
    }
}
