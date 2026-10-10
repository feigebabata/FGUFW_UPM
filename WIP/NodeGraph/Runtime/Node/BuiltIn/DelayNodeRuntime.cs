using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class DelayNodeRuntime : NodeRuntime
    {
        private readonly DelayNodeDefinition definition;
        private float elapsed;

        public DelayNodeRuntime(DelayNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
            this.definition = definition;
        }

        public override float Progress =>
            definition.Duration <= 0f ? 1f : Mathf.Clamp01(elapsed / definition.Duration);

        public override void Tick(float deltaTime)
        {
            elapsed += Mathf.Max(0f, deltaTime);
            if (elapsed >= definition.Duration)
            {
                Complete(DelayNodeDefinition.CompletePortId);
            }
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            elapsed = 0f;
            if (definition.Duration <= 0f)
            {
                Complete(DelayNodeDefinition.CompletePortId);
            }
        }
    }
}
