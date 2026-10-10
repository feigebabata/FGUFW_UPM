using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public abstract class NodeValue
    {
        [SerializeField]
        private NodeValueMode mode;

        [SerializeField]
        private string blackboardKey;

        public NodeValueMode Mode => mode;

        public string BlackboardKey => blackboardKey;

        protected T ResolveBlackboardValue<T>(INodeBlackboard blackboard)
        {
            if (blackboard == null)
            {
                throw new ArgumentNullException(nameof(blackboard));
            }

            return blackboard.Get<T>(blackboardKey);
        }
    }
}
