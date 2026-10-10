namespace FGUFW.NodeGraph
{
    public sealed class EndNodeRuntime : NodeRuntime
    {
        public EndNodeRuntime(EndNodeDefinition definition, NodeGraphExecutor executor) : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            CompleteNodeOnly();
            Executor.CompleteGraph(this);
        }
    }
}
