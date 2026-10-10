using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class ForNodeRuntime : NodeRuntime
    {
        private readonly ForNodeDefinition definition;
        private bool initialized;
        private int remaining;

        public ForNodeRuntime(ForNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
            this.definition = definition;
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            if (!initialized)
            {
                remaining = Mathf.Max(0, definition.Count.Resolve(Executor.Blackboard));
                initialized = true;
            }

            if (remaining <= 0)
            {
                initialized = false;
                Complete(ForNodeDefinition.EndPortId);
                return;
            }

            remaining--;
            Complete(ForNodeDefinition.ItemPortId);
        }

        protected override void ResetForEnter()
        {
            if (State == NodeRuntimeState.Idle)
            {
                initialized = false;
                remaining = 0;
            }
        }
    }
}
