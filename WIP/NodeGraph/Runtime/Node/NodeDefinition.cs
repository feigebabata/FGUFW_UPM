using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    public abstract class NodeDefinition : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private Vector2 position;
        public Vector2 Position => position;
        public abstract IReadOnlyList<NodePortDefinition> Ports { get; }

        public abstract NodeRuntime CreateRuntime(NodeGraphExecutor executor);
        public bool TryGetPort(string portId, out NodePortDefinition portDefinition)
        {
            var ports = Ports;
            if (ports != null)
            {
                for (var i = 0; i < ports.Count; i++)
                {
                    if (string.Equals(ports[i].Id, portId, StringComparison.Ordinal))
                    {
                        portDefinition = ports[i];
                        return true;
                    }
                }
            }

            portDefinition = default;
            return false;
        }

        internal void SetPosition(Vector2 value)
        {
            position = value;
        }
    }
}
