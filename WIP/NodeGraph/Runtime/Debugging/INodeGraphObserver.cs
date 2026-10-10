using System;

namespace FGUFW.NodeGraph
{
    public interface INodeGraphObserver
    {
        void OnGraphStarted(NodeGraphExecutor executor);
        void OnNodeEntered(NodeGraphExecutor executor, NodeRuntime runtime, NodeRuntimeEdge sourceEdge);
        void OnNodeProgress(NodeGraphExecutor executor, NodeRuntime runtime, float progress);
        void OnNodeWaitProgress(NodeGraphExecutor executor, NodeRuntime runtime, int arrived, int total);
        void OnEdgeTriggered(NodeGraphExecutor executor, NodeRuntimeEdge edge);
        void OnNodeCompleted(NodeGraphExecutor executor, NodeRuntime runtime);
        void OnNodeFailed(NodeGraphExecutor executor, NodeRuntime runtime, Exception exception);
        void OnNodeCancelled(NodeGraphExecutor executor, NodeRuntime runtime);
        void OnGraphStopped(NodeGraphExecutor executor, NodeGraphRuntimeStatus status);
    }
}
