using System.Collections.Generic;

namespace FGUFW.NodeGraph.Editor.Tests
{
    internal sealed class EditorTestNode : NodeDefinition
    {
        public const string InputPortId = "enter";
        public const string OutputPortId = "next";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(OutputPortId, "Next", NodePortDirection.Output, typeof(FlowPort))
        };

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new EditorTestNodeRuntime(this, executor);
        }
    }

    internal sealed class EditorTestNodeRuntime : NodeRuntime
    {
        public EditorTestNodeRuntime(EditorTestNode definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            Complete(EditorTestNode.OutputPortId);
        }
    }
}
