using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Flow/For Delay", "For Delay")]
    public sealed class ForDelayNodeDefinition : ProgressNodeDefinition
    {
        public const string InputPortId = "enter";
        public const string ItemPortId = "item";
        public const string EndPortId = "end";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(ItemPortId, "Item", NodePortDirection.Output, typeof(FlowPort)),
            new NodePortDefinition(EndPortId, "End", NodePortDirection.Output, typeof(FlowPort))
        };

        [SerializeField]
        private IntNodeValue count = new IntNodeValue(1);

        [SerializeField]
        private FloatNodeValue delay = new FloatNodeValue(1f);

        public IntNodeValue Count => count;

        public FloatNodeValue Delay => delay;

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new ForDelayNodeRuntime(this, executor);
        }
    }
}
