namespace FGUFW.NodeGraph.TestFixtures
{
    public sealed class DemoDelayNodeRuntime : NodeRuntime
    {
        public DemoDelayNodeRuntime(DemoDelayNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            Complete(DemoDelayNodeDefinition.OutputPortId);
        }
    }
}
