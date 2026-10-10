using System;

namespace FGUFW.NodeGraph
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NodeMenuAttribute : Attribute
    {
        public NodeMenuAttribute(string path, string displayName = null)
        {
            Path = path ?? string.Empty;
            DisplayName = displayName;
        }

        public string Path { get; }
        public string DisplayName { get; }
    }
}
