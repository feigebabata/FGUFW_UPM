using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodeView : Node
    {
        private readonly Dictionary<string, Port> portsById = new Dictionary<string, Port>(StringComparer.Ordinal);
        private readonly List<Port> inputPorts = new List<Port>();
        private readonly ProgressBar progressBar;
        private readonly Label runtimeLabel;

        public NodeView(NodeDefinition definition, IEdgeConnectorListener connectorListener = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var info = NodeTypeRegistry.GetInfo(definition.GetType());
            title = info.DisplayName;
            var ports = definition.Ports;
            for (var i = 0; i < ports.Count; i++)
            {
                var portDefinition = ports[i];
                var direction = portDefinition.Direction == NodePortDirection.Input ? Direction.Input : Direction.Output;
                var port = NodePort.Create(
                    Orientation.Horizontal,
                    direction,
                    Port.Capacity.Multi,
                    portDefinition.ValueType,
                    connectorListener);
                port.portName = portDefinition.DisplayName;
                port.userData = portDefinition.Id;
                portsById.Add(portDefinition.Id, port);
                if (direction == Direction.Input)
                {
                    inputPorts.Add(port);
                    inputContainer.Add(port);
                }
                else
                {
                    outputContainer.Add(port);
                }
            }

            if (definition is StartNodeDefinition)
            {
                titleContainer.AddToClassList("node-title--start");
                ApplyTitleColors(new Color32(18, 122, 53, 255), new Color32(8, 72, 29, 255), new Color32(235, 255, 240, 255));
                capabilities &= ~Capabilities.Copiable;
                capabilities &= ~Capabilities.Deletable;
                style.minWidth = 180f;
            }
            else if (definition is EndNodeDefinition)
            {
                titleContainer.AddToClassList("node-title--end");
                ApplyTitleColors(new Color32(179, 38, 46, 255), new Color32(102, 18, 24, 255), new Color32(255, 238, 238, 255));
                capabilities &= ~Capabilities.Copiable;
                capabilities &= ~Capabilities.Deletable;
                style.minWidth = 180f;
            }
            else
            {
                var inspector = new InspectorElement(new SerializedObject(definition));
                inspector.style.marginTop = 4f;
                extensionContainer.Add(inspector);
                style.minWidth = 230f;
            }

            if (definition is ProgressNodeDefinition)
            {
                progressBar = new ProgressBar
                {
                    lowValue = 0f,
                    highValue = 1f,
                    title = string.Empty
                };
                progressBar.AddToClassList("node-progress");
                progressBar.style.display = DisplayStyle.None;
                extensionContainer.Add(progressBar);
            }

            runtimeLabel = new Label();
            runtimeLabel.style.display = DisplayStyle.None;
            runtimeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            runtimeLabel.style.marginTop = 4f;
            extensionContainer.Add(runtimeLabel);
            var defaultSize = definition is StartNodeDefinition || definition is EndNodeDefinition
                ? new Vector2(180f, 70f)
                : new Vector2(260f, 160f);
            SetPosition(new Rect(definition.Position, defaultSize));
            RefreshExpandedState();
            RefreshPorts();
        }

        public NodeDefinition Definition { get; }

        public Port GetPort(string portId)
        {
            portsById.TryGetValue(portId, out var port);
            return port;
        }

        public string GetPortId(Port port)
        {
            return port?.userData as string;
        }

        public Port GetFirstCompatibleInput(Type portType)
        {
            return inputPorts.FirstOrDefault(port => port.portType == portType);
        }

        public void SetProgress(float progress)
        {
            if (progressBar == null)
            {
                return;
            }

            progressBar.value = Mathf.Clamp01(progress);
            progressBar.title = $"{Mathf.RoundToInt(progressBar.value * 100f)}%";
            progressBar.style.display = DisplayStyle.Flex;
        }

        public void ClearProgress()
        {
            if (progressBar != null)
            {
                progressBar.style.display = DisplayStyle.None;
            }
        }

        public void SetDebugState(NodeGraphDebugState state)
        {
            if (state == null)
            {
                ClearDebugState();
                return;
            }

            var color = GetStateColor(state.State);
            SetDebugBorder(color);
            runtimeLabel.text = state.WaitTotal > 0 ? $"{state.State}  {state.WaitArrived}/{state.WaitTotal}" : state.State.ToString();
            runtimeLabel.tooltip = state.Exception?.ToString();
            runtimeLabel.style.display = DisplayStyle.Flex;
            if (state.State == NodeRuntimeState.Running && state.Progress >= 0f)
            {
                SetProgress(state.Progress);
            }
            else
            {
                ClearProgress();
            }
        }

        public void ClearDebugState()
        {
            runtimeLabel.style.display = DisplayStyle.None;
            runtimeLabel.tooltip = string.Empty;
            ClearProgress();
            style.borderLeftWidth = 0f;
            style.borderRightWidth = 0f;
            style.borderTopWidth = 0f;
            style.borderBottomWidth = 0f;
        }

        private void ApplyTitleColors(Color background, Color border, Color text)
        {
            titleContainer.style.backgroundColor = background;
            titleContainer.style.borderLeftColor = border;
            titleContainer.style.borderRightColor = border;
            titleContainer.style.borderTopColor = border;
            titleContainer.style.borderBottomColor = border;
            var titleLabel = titleContainer.Q<Label>();
            if (titleLabel != null)
            {
                titleLabel.style.color = text;
            }
        }

        private void SetDebugBorder(Color color)
        {
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftWidth = 2f;
            style.borderRightWidth = 2f;
            style.borderTopWidth = 2f;
            style.borderBottomWidth = 2f;
        }

        private static Color GetStateColor(NodeRuntimeState state)
        {
            switch (state)
            {
                case NodeRuntimeState.Running:
                    return new Color32(42, 169, 224, 255);
                case NodeRuntimeState.Completed:
                    return new Color32(52, 190, 100, 255);
                case NodeRuntimeState.Failed:
                    return new Color32(220, 55, 65, 255);
                case NodeRuntimeState.Cancelled:
                    return new Color32(135, 135, 135, 255);
                default:
                    return Color.clear;
            }
        }
    }
}
