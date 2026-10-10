using System.Collections.Generic;

namespace FGUFW.NodeGraph.TestFixtures
{
    public sealed class DemoDelayNodeDefinition : ProgressNodeDefinition
    {
        public const string InputPortId = "enter";
        public const string OutputPortId = "complete";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(OutputPortId, "Complete", NodePortDirection.Output, typeof(FlowPort))
        };

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new DemoDelayNodeRuntime(this, executor);
        }
    }
}
