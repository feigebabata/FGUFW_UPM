using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [NodeMenu("Debug/Log", "Log")]
    public sealed class LogNodeDefinition : NodeDefinition
    {
        public const string InputPortId = "enter";
        public const string NextPortId = "next";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort)),
            new NodePortDefinition(NextPortId, "Next", NodePortDirection.Output, typeof(FlowPort))
        };

        [SerializeField]
        private NodeLogLevel level;

        [SerializeField]
        private StringNodeValue message = new StringNodeValue();

        public NodeLogLevel Level => level;

        public StringNodeValue Message => message;

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new LogNodeRuntime(this, executor);
        }
    }
}
