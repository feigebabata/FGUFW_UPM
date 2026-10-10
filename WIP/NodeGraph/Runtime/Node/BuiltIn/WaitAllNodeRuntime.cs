using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public sealed class WaitAllNodeRuntime : NodeRuntime
    {
        private readonly HashSet<NodeRuntimeEdge> arrivedEdges = new HashSet<NodeRuntimeEdge>();
        public WaitAllNodeRuntime(WaitAllNodeDefinition definition, NodeGraphExecutor executor) : base(definition, executor)
        {
        }

        public override float Progress => InputEdges.Count == 0 ? 0f : (float)arrivedEdges.Count / InputEdges.Count;
        protected override bool AcceptEnterWhileRunning => true;

        protected override void ResetForEnter()
        {
            arrivedEdges.Clear();
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            if (sourceEdge == null)
            {
                Fail("Wait cannot be entered without a source edge.");
                return;
            }

            arrivedEdges.Add(sourceEdge);
            Executor.NotifyNodeWaitProgress(this, arrivedEdges.Count, InputEdges.Count);
            if (arrivedEdges.Count >= InputEdges.Count)
            {
                Complete(WaitAllNodeDefinition.CompletePortId);
            }
        }
    }
}
