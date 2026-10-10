using System.Collections.Generic;

namespace FGUFW.NodeGraph.Editor.Tests
{
    internal sealed class EditorProgressTestNode : ProgressNodeDefinition
    {
        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions =
            new NodePortDefinition[0];

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new EditorProgressTestNodeRuntime(this, executor);
        }
    }

    internal sealed class EditorProgressTestNodeRuntime : NodeRuntime
    {
        public EditorProgressTestNodeRuntime(EditorProgressTestNode definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            CompleteNodeOnly();
        }
    }
}
