using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class NodeExecutionContext
    {
        public NodeExecutionContext(NodeGraphExecutor executor, INodeBlackboard blackboard, Object owner, object userData)
        {
            Executor = executor;
            Blackboard = blackboard;
            Owner = owner;
            UserData = userData;
        }

        public NodeGraphExecutor Executor { get; }
        public INodeBlackboard Blackboard { get; }
        public Object Owner { get; }
        public object UserData { get; }
    }
}
