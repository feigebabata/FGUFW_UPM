using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodeEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly NodeGraphView graphView;

        public NodeEdgeConnectorListener(NodeGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            var draggedPort =
                (edge.output != null ? edge.output.edgeConnector.edgeDragHelper.draggedPort : null)
                ?? (edge.input != null ? edge.input.edgeConnector.edgeDragHelper.draggedPort : null);

            if (draggedPort == null || draggedPort.direction != Direction.Output)
            {
                return;
            }

            graphView.OpenNodeSearch(draggedPort, position);
        }

        public void OnDrop(GraphView targetGraphView, Edge edge)
        {
            graphView.AddEdgeFromConnector(edge);
        }
    }
}
