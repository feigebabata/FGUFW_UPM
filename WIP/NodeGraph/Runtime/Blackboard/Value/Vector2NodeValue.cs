using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class Vector2NodeValue : NodeValue
    {
        [SerializeField]
        private Vector2 constantValue;

        public Vector2 ConstantValue => constantValue;

        public Vector2 Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<Vector2>(blackboard);
        }
    }
}
