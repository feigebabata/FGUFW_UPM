using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodePort : Port
    {
        private NodePort(
            Orientation orientation,
            Direction direction,
            Capacity capacity,
            Type type,
            IEdgeConnectorListener listener)
            : base(orientation, direction, capacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(listener ?? NullEdgeConnectorListener.Instance);
            VisualElementExtensions.AddManipulator(this, m_EdgeConnector);
        }

        public static NodePort Create(
            Orientation orientation,
            Direction direction,
            Capacity capacity,
            Type type,
            IEdgeConnectorListener listener)
        {
            return new NodePort(orientation, direction, capacity, type, listener);
        }

        private sealed class NullEdgeConnectorListener : IEdgeConnectorListener
        {
            public static readonly NullEdgeConnectorListener Instance = new NullEdgeConnectorListener();

            public void OnDropOutsidePort(Edge edge, UnityEngine.Vector2 position)
            {
            }

            public void OnDrop(GraphView graphView, Edge edge)
            {
            }
        }
    }
}
