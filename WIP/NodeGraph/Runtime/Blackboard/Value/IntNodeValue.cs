using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class IntNodeValue : NodeValue
    {
        [SerializeField]
        private int constantValue;

        public IntNodeValue()
        {
        }

        public IntNodeValue(int constantValue)
        {
            this.constantValue = constantValue;
        }

        public int ConstantValue => constantValue;

        public int Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<int>(blackboard);
        }
    }
}
