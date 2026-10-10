using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [DisallowMultipleComponent]
    public sealed class NodeBlackboardComponent : MonoBehaviour
    {
        [SerializeField]
        private List<NodeBlackboardEntry> entries = new List<NodeBlackboardEntry>();

        public IReadOnlyList<NodeBlackboardEntry> Entries => entries;

        public void CopyTo(INodeBlackboard blackboard, bool overwriteExisting = false)
        {
            if (blackboard == null)
            {
                throw new ArgumentNullException(nameof(blackboard));
            }

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                if (!overwriteExisting && blackboard.Contains(entry.Key))
                {
                    continue;
                }

                blackboard.Set(entry.Key, entry.GetValue());
            }
        }
    }
}
