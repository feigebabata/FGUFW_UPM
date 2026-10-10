using System;
using System.Collections.Generic;

namespace FGUFW.NodeGraph
{
    public static class NodeGraphExecutorRegistry
    {
        private static readonly HashSet<NodeGraphExecutor> activeExecutors = new HashSet<NodeGraphExecutor>();
        public static event Action<NodeGraphExecutor> ExecutorCreated;
        public static event Action<NodeGraphExecutor> ExecutorDisposed;
        public static IReadOnlyCollection<NodeGraphExecutor> ActiveExecutors => activeExecutors;

        internal static void Register(NodeGraphExecutor executor)
        {
            if (activeExecutors.Add(executor))
            {
                ExecutorCreated?.Invoke(executor);
            }
        }

        internal static void Unregister(NodeGraphExecutor executor)
        {
            if (activeExecutors.Remove(executor))
            {
                ExecutorDisposed?.Invoke(executor);
            }
        }
    }
}
