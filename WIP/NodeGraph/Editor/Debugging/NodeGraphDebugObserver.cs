using System;
using System.Collections.Generic;
using UnityEditor;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodeGraphDebugState
    {
        public NodeRuntimeState State;
        public float Progress = -1f;
        public int WaitArrived;
        public int WaitTotal;
        public Exception Exception;
    }

    internal sealed class NodeGraphDebugObserver : INodeGraphObserver, IDisposable
    {
        private readonly Dictionary<NodeDefinition, NodeGraphDebugState> nodeStates = new Dictionary<NodeDefinition, NodeGraphDebugState>();
        private readonly Dictionary<NodeEdge, double> edgeTriggerTimes = new Dictionary<NodeEdge, double>();
        public NodeGraphDebugObserver(NodeGraphExecutor executor)
        {
            Executor = executor;
            Executor.AddObserver(this);
            Executor.Blackboard.ValueChanged += OnBlackboardValueChanged;
        }

        public NodeGraphExecutor Executor { get; }
        public int Revision { get; private set; }

        public bool TryGetNodeState(NodeDefinition definition, out NodeGraphDebugState state)
        {
            return nodeStates.TryGetValue(definition, out state);
        }

        public bool WasEdgeTriggeredRecently(NodeEdge edge, double duration)
        {
            return edgeTriggerTimes.TryGetValue(edge, out var time) && EditorApplication.timeSinceStartup - time <= duration;
        }

        public void OnGraphStarted(NodeGraphExecutor executor)
        {
            nodeStates.Clear();
            edgeTriggerTimes.Clear();
            Touch();
        }

        public void OnNodeEntered(NodeGraphExecutor executor, NodeRuntime runtime, NodeRuntimeEdge sourceEdge)
        {
            var state = GetState(runtime);
            state.State = NodeRuntimeState.Running;
            state.Exception = null;
            state.Progress = runtime.Progress;
            Touch();
        }

        public void OnNodeProgress(NodeGraphExecutor executor, NodeRuntime runtime, float progress)
        {
            var state = GetState(runtime);
            state.State = runtime.State;
            state.Progress = progress;
            Touch();
        }

        public void OnNodeWaitProgress(NodeGraphExecutor executor, NodeRuntime runtime, int arrived, int total)
        {
            var state = GetState(runtime);
            state.State = runtime.State;
            state.Progress = runtime.Progress;
            state.WaitArrived = arrived;
            state.WaitTotal = total;
            Touch();
        }

        public void OnEdgeTriggered(NodeGraphExecutor executor, NodeRuntimeEdge edge)
        {
            var edgeData = new NodeEdge(edge.OutputNode.Definition, edge.OutputPortId, edge.InputNode.Definition, edge.InputPortId);
            edgeTriggerTimes[edgeData] = EditorApplication.timeSinceStartup;
            Touch();
        }

        public void OnNodeCompleted(NodeGraphExecutor executor, NodeRuntime runtime)
        {
            var state = GetState(runtime);
            state.State = NodeRuntimeState.Completed;
            state.Progress = runtime.Progress;
            Touch();
        }

        public void OnNodeFailed(NodeGraphExecutor executor, NodeRuntime runtime, Exception exception)
        {
            var state = GetState(runtime);
            state.State = NodeRuntimeState.Failed;
            state.Exception = exception;
            Touch();
        }

        public void OnNodeCancelled(NodeGraphExecutor executor, NodeRuntime runtime)
        {
            var state = GetState(runtime);
            state.State = NodeRuntimeState.Cancelled;
            Touch();
        }

        public void OnGraphStopped(NodeGraphExecutor executor, NodeGraphRuntimeStatus status)
        {
            Touch();
        }

        public void Dispose()
        {
            Executor.RemoveObserver(this);
            Executor.Blackboard.ValueChanged -= OnBlackboardValueChanged;
        }

        private NodeGraphDebugState GetState(NodeRuntime runtime)
        {
            if (!nodeStates.TryGetValue(runtime.Definition, out var state))
            {
                state = new NodeGraphDebugState();
                nodeStates.Add(runtime.Definition, state);
            }

            return state;
        }

        private void OnBlackboardValueChanged(string key, object previous, object value)
        {
            Touch();
        }

        private void Touch()
        {
            Revision++;
        }
    }
}
