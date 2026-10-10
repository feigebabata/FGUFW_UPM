using System;
using System.Collections.Generic;
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
                Debug.LogError($"Cannot resolve {typeof(T).Name} node value because the Blackboard is null.");
                throw new ArgumentNullException(nameof(blackboard));
            }

            if (string.IsNullOrWhiteSpace(blackboardKey))
            {
                Debug.LogError($"Cannot resolve {typeof(T).Name} node value because the Blackboard key is empty.");
                throw new InvalidOperationException("Blackboard key cannot be empty.");
            }

            if (blackboard.TryGet<T>(blackboardKey, out var value))
            {
                return value;
            }

            if (!blackboard.Contains(blackboardKey))
            {
                Debug.LogError($"Cannot resolve {typeof(T).Name} node value because Blackboard key '{blackboardKey}' does not exist.");
                throw new KeyNotFoundException($"Blackboard key '{blackboardKey}' does not exist.");
            }

            Debug.LogError($"Cannot resolve Blackboard key '{blackboardKey}' as {typeof(T).Name} because its value has a different type.");
            throw new InvalidCastException($"Blackboard key '{blackboardKey}' is not {typeof(T).Name}.");
        }
    }
}
