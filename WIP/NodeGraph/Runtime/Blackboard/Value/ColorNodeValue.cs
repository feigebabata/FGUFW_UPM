using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class ColorNodeValue : NodeValue
    {
        [SerializeField]
        private Color constantValue = Color.white;

        public Color ConstantValue => constantValue;

        public Color Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<Color>(blackboard);
        }
    }
}
