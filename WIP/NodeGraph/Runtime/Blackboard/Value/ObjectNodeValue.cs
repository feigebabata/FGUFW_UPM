using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class ObjectNodeValue : NodeValue
    {
        [SerializeField]
        private UnityEngine.Object constantValue;

        public UnityEngine.Object ConstantValue => constantValue;

        public UnityEngine.Object Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant
                ? constantValue
                : ResolveBlackboardValue<UnityEngine.Object>(blackboard);
        }
    }
}
