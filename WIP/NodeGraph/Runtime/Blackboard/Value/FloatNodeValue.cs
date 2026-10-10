using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class FloatNodeValue : NodeValue
    {
        [SerializeField]
        private float constantValue;

        public float ConstantValue => constantValue;

        public float Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<float>(blackboard);
        }
    }
}
