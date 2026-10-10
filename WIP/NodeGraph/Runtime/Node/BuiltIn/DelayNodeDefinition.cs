using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Flow/Delay", "Delay")]
    public sealed class DelayNodeDefinition : ProgressNodeDefinition
    {
        public const string InputPortId = "enter";
        public const string CompletePortId = "complete";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(CompletePortId, "Complete", NodePortDirection.Output, typeof(FlowPort))
        };

        [SerializeField]
        private FloatNodeValue duration = new FloatNodeValue();

        public FloatNodeValue Duration => duration;

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new DelayNodeRuntime(this, executor);
        }
    }
}
