using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class BoolNodeValue : NodeValue
    {
        [SerializeField]
        private bool constantValue;

        public bool ConstantValue => constantValue;

        public bool Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<bool>(blackboard);
        }
    }
}
