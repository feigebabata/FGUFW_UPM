using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class FloatNodeValue : NodeValue
    {
        [SerializeField]
        private float constantValue;

        public FloatNodeValue()
        {
        }

        public FloatNodeValue(float constantValue)
        {
            this.constantValue = constantValue;
        }

        public float ConstantValue => constantValue;

        public float Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<float>(blackboard);
        }
    }
}
