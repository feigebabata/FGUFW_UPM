using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class LogNodeRuntime : NodeRuntime
    {
        private readonly LogNodeDefinition definition;

        public LogNodeRuntime(LogNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
            this.definition = definition;
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
            switch (definition.Level)
            {
                case NodeLogLevel.Warning:
                    Debug.LogWarning(definition.Message, Context.Owner);
                    break;
                case NodeLogLevel.Error:
                    Debug.LogError(definition.Message, Context.Owner);
                    break;
                default:
                    Debug.Log(definition.Message, Context.Owner);
                    break;
            }

            Complete(LogNodeDefinition.NextPortId);
        }
    }
}
