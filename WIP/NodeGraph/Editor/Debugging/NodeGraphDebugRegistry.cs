using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace FGUFW.NodeGraph.Editor
{
    [InitializeOnLoad]
    internal static class NodeGraphDebugRegistry
    {
        private static readonly Dictionary<NodeGraphExecutor, NodeGraphDebugObserver> observers =
            new Dictionary<NodeGraphExecutor, NodeGraphDebugObserver>();
        static NodeGraphDebugRegistry()
        {
            NodeGraphExecutorRegistry.ExecutorCreated += OnExecutorCreated;
            NodeGraphExecutorRegistry.ExecutorDisposed += OnExecutorDisposed;
            foreach (var executor in NodeGraphExecutorRegistry.ActiveExecutors)
            {
                OnExecutorCreated(executor);
            }
        }

        public static event Action Changed;
        public static IReadOnlyCollection<NodeGraphDebugObserver> Observers => observers.Values;

        public static IEnumerable<NodeGraphDebugObserver> ForGraph(NodeGraphAsset graph)
        {
            return observers.Values.Where(observer => observer.Executor.Graph == graph);
        }

        public static NodeGraphDebugObserver GetObserver(NodeGraphExecutor executor)
        {
            if (executor == null)
            {
                return null;
            }

            observers.TryGetValue(executor, out var observer);
            return observer;
        }

        private static void OnExecutorCreated(NodeGraphExecutor executor)
        {
            if (executor == null || observers.ContainsKey(executor))
            {
                return;
            }

            observers.Add(executor, new NodeGraphDebugObserver(executor));
            Changed?.Invoke();
        }

        private static void OnExecutorDisposed(NodeGraphExecutor executor)
        {
            if (!observers.TryGetValue(executor, out var observer))
            {
                return;
            }

            observer.Dispose();
            observers.Remove(executor);
            Changed?.Invoke();
        }
    }
}
