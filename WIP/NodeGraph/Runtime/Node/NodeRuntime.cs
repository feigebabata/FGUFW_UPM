using System;
using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public abstract class NodeRuntime
    {
        private readonly List<NodeRuntimeEdge> inputEdges = new List<NodeRuntimeEdge>();
        private readonly List<NodeRuntimeEdge> outputEdges = new List<NodeRuntimeEdge>();
        protected NodeRuntime(NodeDefinition definition, NodeGraphExecutor executor)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public NodeDefinition Definition { get; }
        public NodeGraphExecutor Executor { get; }
        public IReadOnlyList<NodeRuntimeEdge> InputEdges => inputEdges;
        public IReadOnlyList<NodeRuntimeEdge> OutputEdges => outputEdges;
        public NodeRuntimeState State { get; private set; }
        public virtual float Progress => -1f;
        protected virtual bool AcceptEnterWhileRunning => false;

        internal void AddInput(NodeRuntimeEdge edge)
        {
            inputEdges.Add(edge);
        }

        internal void AddOutput(NodeRuntimeEdge edge)
        {
            outputEdges.Add(edge);
        }

        internal void Enter(NodeRuntimeEdge sourceEdge)
        {
            if (Executor.Status != NodeGraphRuntimeStatus.Running)
            {
                return;
            }

            if (State == NodeRuntimeState.Running)
            {
                if (!AcceptEnterWhileRunning)
                {
                    Executor.FailGraph($"Node '{Definition.name}' was entered while already running.", null, this);
                    return;
                }

                Executor.NotifyNodeEntered(this, sourceEdge);
                InvokeEnter(sourceEdge);
                return;
            }

            ResetForEnter();
            State = NodeRuntimeState.Running;
            Executor.AddRunningNode(this);
            Executor.NotifyNodeEntered(this, sourceEdge);
            InvokeEnter(sourceEdge);
        }

        public virtual void Tick(float deltaTime)
        {
        }

        internal void TickSafely(float deltaTime)
        {
            if (State != NodeRuntimeState.Running)
            {
                return;
            }

            try
            {
                Tick(deltaTime);
                if (State == NodeRuntimeState.Running && Progress >= 0f)
                {
                    Executor.NotifyNodeProgress(this, Progress);
                }
            }
            catch (Exception exception)
            {
                Fail($"Node '{Definition.name}' threw during Tick.", exception);
            }
        }

        public virtual void Cancel()
        {
            if (State != NodeRuntimeState.Running)
            {
                return;
            }

            State = NodeRuntimeState.Cancelled;
            try
            {
                OnCancelled();
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);
            }

            Executor.NotifyNodeCancelled(this);
        }

        protected abstract void OnEnter(NodeRuntimeEdge sourceEdge);
        protected virtual void ResetForEnter()
        {
        }

        protected virtual void OnCancelled()
        {
        }

        protected void Complete(params string[] outputPortIds)
        {
            if (!CompleteNodeOnly() || outputPortIds == null || outputPortIds.Length == 0)
            {
                return;
            }

            var selectedPorts = new HashSet<string>(outputPortIds, StringComparer.Ordinal);
            foreach (var edge in outputEdges)
            {
                if (selectedPorts.Contains(edge.OutputPortId))
                {
                    Executor.Enqueue(edge);
                }
            }
        }

        protected bool CompleteNodeOnly()
        {
            if (State != NodeRuntimeState.Running)
            {
                return false;
            }

            State = NodeRuntimeState.Completed;
            Executor.RemoveRunningNode(this);
            Executor.NotifyNodeCompleted(this);
            return true;
        }

        protected void Fail(string message, Exception exception = null)
        {
            if (State == NodeRuntimeState.Failed)
            {
                return;
            }

            State = NodeRuntimeState.Failed;
            Executor.RemoveRunningNode(this);
            Executor.NotifyNodeFailed(this, exception);
            Executor.FailGraph(message, exception, this);
        }

        internal void MarkFailed(Exception exception)
        {
            State = NodeRuntimeState.Failed;
            Executor.RemoveRunningNode(this);
            Executor.NotifyNodeFailed(this, exception);
        }

        internal void ResetRuntime()
        {
            State = NodeRuntimeState.Idle;
            ResetForEnter();
        }

        private void InvokeEnter(NodeRuntimeEdge sourceEdge)
        {
            try
            {
                OnEnter(sourceEdge);
            }
            catch (Exception exception)
            {
                Fail($"Node '{Definition.name}' threw during Enter.", exception);
            }
        }
    }
}
