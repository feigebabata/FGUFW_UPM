using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    [Serializable]
    public sealed class StringNodeValue : NodeValue
    {
        [SerializeField]
        private string constantValue;

        public string ConstantValue => constantValue;

        public string Resolve(INodeBlackboard blackboard)
        {
            return Mode == NodeValueMode.Constant ? constantValue : ResolveBlackboardValue<string>(blackboard);
        }
    }
}
