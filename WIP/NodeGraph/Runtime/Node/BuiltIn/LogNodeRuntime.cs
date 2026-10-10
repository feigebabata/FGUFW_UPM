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
            var message = definition.Message.Resolve(Context.Blackboard);
            switch (definition.Level)
            {
                case NodeLogLevel.Warning:
                    Debug.LogWarning(message, Context.Owner);
                    break;
                case NodeLogLevel.Error:
                    Debug.LogError(message, Context.Owner);
                    break;
                default:
                    Debug.Log(message, Context.Owner);
                    break;
            }

            Complete(LogNodeDefinition.NextPortId);
        }
    }
}
