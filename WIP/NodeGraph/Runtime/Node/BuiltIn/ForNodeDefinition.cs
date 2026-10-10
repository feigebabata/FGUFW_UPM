using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Flow/For", "For")]
    public sealed class ForNodeDefinition : NodeDefinition
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

        public IntNodeValue Count => count;

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new ForNodeRuntime(this, executor);
        }
    }
}
