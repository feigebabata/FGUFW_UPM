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
            definition.Duration.Resolve(Context.Blackboard) <= 0f ? 1f : Mathf.Clamp01(elapsed / definition.Duration.Resolve(Context.Blackboard));

        public override void Tick(float deltaTime)
        {
            elapsed += Mathf.Max(0f, deltaTime);
            if (elapsed >= definition.Duration.Resolve(Context.Blackboard))
            {
                Complete(DelayNodeDefinition.CompletePortId);
            }
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            elapsed = 0f;
            if (definition.Duration.Resolve(Context.Blackboard) <= 0f)
            {
                Complete(DelayNodeDefinition.CompletePortId);
            }
        }
    }
}
