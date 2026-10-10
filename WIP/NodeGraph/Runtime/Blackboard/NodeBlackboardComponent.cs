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

#if UNITY_EDITOR
        [NonSerialized]
        private int editorRevision;
#endif

        public IReadOnlyList<NodeBlackboardEntry> Entries => entries;

#if UNITY_EDITOR
        internal int EditorRevision => editorRevision;
#endif

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

#if UNITY_EDITOR
        private void OnValidate()
        {
            unchecked
            {
                editorRevision++;
            }
        }
#endif
    }
}
