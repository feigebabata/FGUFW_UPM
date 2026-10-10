using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    public enum NodeLogLevel
    {
        Log,
        Warning,
        Error
    }

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

        [SerializeField, TextArea]
        private string message;

        public NodeLogLevel Level => level;

        public string Message => message;

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new LogNodeRuntime(this, executor);
        }

        internal void Configure(NodeLogLevel valueLevel, string valueMessage)
        {
            level = valueLevel;
            message = valueMessage;
        }
    }
}
