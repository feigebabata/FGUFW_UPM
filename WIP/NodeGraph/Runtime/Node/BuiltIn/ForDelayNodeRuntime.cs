using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class ForDelayNodeRuntime : NodeRuntime
    {
        private readonly ForDelayNodeDefinition definition;
        private float delay;
        private float elapsed;
        private int remaining;
        private int total;

        public ForDelayNodeRuntime(ForDelayNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
            this.definition = definition;
        }

        public override float Progress => total <= 0 ? 1f : (float)(total - remaining) / total;

        public override void Tick(float deltaTime)
        {
            elapsed += Mathf.Max(0f, deltaTime);
            if (delay > 0f && elapsed < delay)
            {
                return;
            }

            elapsed = 0f;
            TriggerItem();
            remaining--;
            if (remaining <= 0)
            {
                Complete(ForDelayNodeDefinition.EndPortId);
            }
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            total = Mathf.Max(0, definition.Count.Resolve(Executor.Blackboard));
            remaining = total;
            delay = Mathf.Max(0f, definition.Delay.Resolve(Executor.Blackboard));
            elapsed = 0f;
            if (remaining == 0)
            {
                Complete(ForDelayNodeDefinition.EndPortId);
            }
        }

        protected override void ResetForEnter()
        {
            delay = 0f;
            elapsed = 0f;
            remaining = 0;
            total = 0;
        }

        private void TriggerItem()
        {
            foreach (var edge in OutputEdges)
            {
                if (edge.OutputPortId == ForDelayNodeDefinition.ItemPortId)
                {
                    Executor.Enqueue(edge);
                }
            }
        }
    }
}
