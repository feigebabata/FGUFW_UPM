namespace FGUFW.NodeGraph
{
    public sealed class StartNodeRuntime : NodeRuntime
    {
        public StartNodeRuntime(StartNodeDefinition definition, NodeGraphExecutor executor) : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            Complete(StartNodeDefinition.StartPortId);
        }
    }
}
