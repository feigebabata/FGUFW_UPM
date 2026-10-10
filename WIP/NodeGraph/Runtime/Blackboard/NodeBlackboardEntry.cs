using System;
using UnityEngine;

namespace FGUFW.NodeGraph
{
    public enum NodeBlackboardValueType
    {
        Bool,
        Int,
        Float,
        String,
        Vector2,
        Vector3,
        Color,
        Object
    }

    [Serializable]
    public sealed class NodeBlackboardEntry
    {
        [SerializeField]
        private string key;

        [SerializeField]
        private NodeBlackboardValueType type;

        [SerializeField]
        private bool boolValue;

        [SerializeField]
        private int intValue;

        [SerializeField]
        private float floatValue;

        [SerializeField]
        private string stringValue;

        [SerializeField]
        private Vector2 vector2Value;

        [SerializeField]
        private Vector3 vector3Value;

        [SerializeField]
        private Color colorValue = Color.white;

        [SerializeField]
        private UnityEngine.Object objectValue;

        public string Key => key;

        public NodeBlackboardValueType Type => type;

        public object GetValue()
        {
            switch (type)
            {
                case NodeBlackboardValueType.Bool:
                    return boolValue;
                case NodeBlackboardValueType.Int:
                    return intValue;
                case NodeBlackboardValueType.Float:
                    return floatValue;
                case NodeBlackboardValueType.String:
                    return stringValue;
                case NodeBlackboardValueType.Vector2:
                    return vector2Value;
                case NodeBlackboardValueType.Vector3:
                    return vector3Value;
                case NodeBlackboardValueType.Color:
                    return colorValue;
                case NodeBlackboardValueType.Object:
                    return objectValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
