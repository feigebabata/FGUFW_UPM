using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class Vector3NodeValue : NodeValue
    {
        [SerializeField]
        private Vector3 constantValue;

        public Vector3 ConstantValue => constantValue;

        public Vector3 Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<Vector3>(blackboard);
        }
    }
}
