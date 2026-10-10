using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class NodeGraphExecutor : IDisposable
    {
        private static int nextExecutorId;
        private readonly Dictionary<NodeDefinition, NodeRuntime> runtimes = new Dictionary<NodeDefinition, NodeRuntime>();
        private readonly List<NodeRuntimeEdge> runtimeEdges = new List<NodeRuntimeEdge>();
        private readonly HashSet<NodeRuntime> runningNodes = new HashSet<NodeRuntime>();
        private readonly Queue<NodeRuntimeEdge> pendingEdges = new Queue<NodeRuntimeEdge>();
        private readonly List<INodeGraphObserver> observers = new List<INodeGraphObserver>();
        private bool disposed;
        public NodeGraphExecutor(
            NodeGraphAsset graph,
            INodeBlackboard blackboard,
            UnityEngine.Object owner = null,
            object userData = null,
            string debugName = null)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            Owner = owner;
            UserData = userData;
            ExecutorId = Interlocked.Increment(ref nextExecutorId);
            DebugName = string.IsNullOrWhiteSpace(debugName) ? $"Executor #{ExecutorId}" : debugName;
            var validation = NodeGraphValidator.Validate(graph);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            CreateRuntimes();
            CreateRuntimeEdges();
            NodeGraphExecutorRegistry.Register(this);
        }

        public int ExecutorId { get; }
        public string DebugName { get; set; }
        public NodeGraphAsset Graph { get; }
        public INodeBlackboard Blackboard { get; }
        public UnityEngine.Object Owner { get; }
        public object UserData { get; }
        public NodeGraphRuntimeStatus Status { get; private set; }
        public Exception LastException { get; private set; }
        public string LastError { get; private set; }
        public int MaxDispatchPerTick { get; set; } = 1024;
        public IReadOnlyDictionary<NodeDefinition, NodeRuntime> Runtimes => runtimes;
        public IReadOnlyList<NodeRuntimeEdge> RuntimeEdges => runtimeEdges;

        public void Start()
        {
            ThrowIfDisposed();
            if (Status == NodeGraphRuntimeStatus.Running || Status == NodeGraphRuntimeStatus.Paused)
            {
                throw new InvalidOperationException("Graph is already running.");
            }

            pendingEdges.Clear();
            runningNodes.Clear();
            LastException = null;
            LastError = null;
            foreach (var runtime in runtimes.Values)
            {
                runtime.ResetRuntime();
            }

            Status = NodeGraphRuntimeStatus.Running;
            Notify(observer => observer.OnGraphStarted(this));
            var startRuntime = runtimes[Graph.GetStartNode()];
            startRuntime.Enter(null);
            ProcessPendingEdges();
            DetectStall();
        }

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();
            if (Status != NodeGraphRuntimeStatus.Running)
            {
                return;
            }

            var snapshot = runningNodes.ToArray();
            foreach (var runtime in snapshot)
            {
                if (Status != NodeGraphRuntimeStatus.Running)
                {
                    break;
                }

                runtime.TickSafely(deltaTime);
            }

            ProcessPendingEdges();
            DetectStall();
        }

        public void Pause()
        {
            if (Status == NodeGraphRuntimeStatus.Running)
            {
                Status = NodeGraphRuntimeStatus.Paused;
            }
        }

        public void Resume()
        {
            if (Status == NodeGraphRuntimeStatus.Paused)
            {
                Status = NodeGraphRuntimeStatus.Running;
            }
        }

        public void Cancel()
        {
            if (Status != NodeGraphRuntimeStatus.Running && Status != NodeGraphRuntimeStatus.Paused)
            {
                return;
            }

            Stop(NodeGraphRuntimeStatus.Cancelled);
        }

        public void AddObserver(INodeGraphObserver observer)
        {
            if (observer != null && !observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public void RemoveObserver(INodeGraphObserver observer)
        {
            observers.Remove(observer);
        }

        public NodeRuntime GetRuntime(NodeDefinition definition)
        {
            runtimes.TryGetValue(definition, out var runtime);
            return runtime;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Cancel();
            observers.Clear();
            NodeGraphExecutorRegistry.Unregister(this);
            disposed = true;
        }

        internal void AddRunningNode(NodeRuntime runtime)
        {
            runningNodes.Add(runtime);
        }

        internal void RemoveRunningNode(NodeRuntime runtime)
        {
            runningNodes.Remove(runtime);
        }

        internal void Enqueue(NodeRuntimeEdge edge)
        {
            if (Status == NodeGraphRuntimeStatus.Running)
            {
                pendingEdges.Enqueue(edge);
            }
        }

        internal void CompleteGraph(NodeRuntime endRuntime)
        {
            if (Status != NodeGraphRuntimeStatus.Running)
            {
                return;
            }

            Stop(NodeGraphRuntimeStatus.Completed, endRuntime);
        }

        internal void FailGraph(string message, Exception exception = null, NodeRuntime failedRuntime = null)
        {
            if (IsTerminal(Status))
            {
                return;
            }

            if (failedRuntime != null && failedRuntime.State != NodeRuntimeState.Failed)
            {
                failedRuntime.MarkFailed(exception);
            }

            LastError = message;
            LastException = exception;
            Stop(NodeGraphRuntimeStatus.Failed, failedRuntime);
        }

        internal void NotifyNodeEntered(NodeRuntime runtime, NodeRuntimeEdge sourceEdge)
        {
            Notify(observer => observer.OnNodeEntered(this, runtime, sourceEdge));
        }

        internal void NotifyNodeProgress(NodeRuntime runtime, float progress)
        {
            Notify(observer => observer.OnNodeProgress(this, runtime, progress));
        }

        internal void NotifyNodeWaitProgress(NodeRuntime runtime, int arrived, int total)
        {
            Notify(observer => observer.OnNodeWaitProgress(this, runtime, arrived, total));
        }

        internal void NotifyNodeCompleted(NodeRuntime runtime)
        {
            Notify(observer => observer.OnNodeCompleted(this, runtime));
        }

        internal void NotifyNodeFailed(NodeRuntime runtime, Exception exception)
        {
            Notify(observer => observer.OnNodeFailed(this, runtime, exception));
        }

        internal void NotifyNodeCancelled(NodeRuntime runtime)
        {
            Notify(observer => observer.OnNodeCancelled(this, runtime));
        }

        private void CreateRuntimes()
        {
            foreach (var definition in Graph.Nodes)
            {
                var runtime = definition.CreateRuntime(this);
                if (runtime == null)
                {
                    throw new InvalidOperationException($"Node '{definition.name}' returned a null runtime.");
                }

                runtimes.Add(definition, runtime);
            }
        }

        private void CreateRuntimeEdges()
        {
            foreach (var edge in Graph.Edges)
            {
                var runtimeEdge = new NodeRuntimeEdge(
                    runtimes[edge.OutputNode],
                    edge.OutputPortId,
                    runtimes[edge.InputNode],
                    edge.InputPortId);
                runtimeEdge.OutputNode.AddOutput(runtimeEdge);
                runtimeEdge.InputNode.AddInput(runtimeEdge);
                runtimeEdges.Add(runtimeEdge);
            }
        }

        private void ProcessPendingEdges()
        {
            var dispatchCount = 0;
            while (pendingEdges.Count > 0 && Status == NodeGraphRuntimeStatus.Running)
            {
                if (++dispatchCount > MaxDispatchPerTick)
                {
                    FailGraph("Too many immediate node transitions.");
                    return;
                }

                var edge = pendingEdges.Dequeue();
                Notify(observer => observer.OnEdgeTriggered(this, edge));
                edge.InputNode.Enter(edge);
            }
        }

        private void DetectStall()
        {
            if (Status != NodeGraphRuntimeStatus.Running || pendingEdges.Count != 0 || runningNodes.Count != 0)
            {
                return;
            }

            Stop(NodeGraphRuntimeStatus.Stalled);
        }

        private void Stop(NodeGraphRuntimeStatus status, NodeRuntime except = null)
        {
            Status = status;
            pendingEdges.Clear();
            CancelRunningNodes(except);
            runningNodes.Clear();
            Notify(observer => observer.OnGraphStopped(this, Status));
        }

        private void CancelRunningNodes(NodeRuntime except)
        {
            foreach (var runtime in runningNodes.ToArray())
            {
                if (runtime == except)
                {
                    continue;
                }

                try
                {
                    runtime.Cancel();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private void Notify(Action<INodeGraphObserver> callback)
        {
            foreach (var observer in observers.ToArray())
            {
                try
                {
                    callback(observer);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        private static bool IsTerminal(NodeGraphRuntimeStatus status)
        {
            return status == NodeGraphRuntimeStatus.Completed
                || status == NodeGraphRuntimeStatus.Failed
                || status == NodeGraphRuntimeStatus.Cancelled
                || status == NodeGraphRuntimeStatus.Stalled;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(NodeGraphExecutor));
            }
        }
    }
}
