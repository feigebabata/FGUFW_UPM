using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodeSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private NodeGraphEditorWindow window;
        private NodeGraphView graphView;
        private Port connectedOutput;
        private Vector2? connectedGraphPosition;
        public void Initialize(NodeGraphEditorWindow owner, NodeGraphView ownerGraphView)
        {
            window = owner;
            graphView = ownerGraphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var root = new MenuGroup("Create Node");
            var types = NodeTypeRegistry.GetCreatableNodeTypes();
            for (var i = 0; i < types.Count; i++)
            {
                if (connectedOutput == null || HasCompatibleInput(types[i].Type, connectedOutput.portType))
                {
                    AddType(root, types[i]);
                }
            }

            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(root.Name), 0)
            };
            AppendGroup(entries, root, 0);
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (!(searchTreeEntry.userData is Type nodeType) || graphView == null || window == null)
            {
                return false;
            }

            var output = connectedOutput;
            var explicitGraphPosition = connectedGraphPosition;
            connectedOutput = null;
            connectedGraphPosition = null;
            var windowPosition = context.screenMousePosition - window.position.position;
            var graphPosition = explicitGraphPosition
                ?? VisualElementExtensions.WorldToLocal(graphView.contentViewContainer, windowPosition);
            graphView.CreateNode(nodeType, graphPosition, output);
            return true;
        }

        public void SetConnectedOutput(Port output, Vector2 graphPosition)
        {
            connectedOutput = output;
            connectedGraphPosition = graphPosition;
        }

        public void ClearConnectedOutput()
        {
            connectedOutput = null;
            connectedGraphPosition = null;
        }

        private static bool HasCompatibleInput(Type nodeType, Type portType)
        {
            var definition = ScriptableObject.CreateInstance(nodeType) as NodeDefinition;
            if (definition == null)
            {
                return false;
            }

            try
            {
                return definition.Ports.Any(port =>
                    port.Direction == NodePortDirection.Input
                    && port.ValueType == portType);
            }
            finally
            {
                DestroyImmediate(definition);
            }
        }

        private static void AddType(MenuGroup root, NodeTypeInfo typeInfo)
        {
            var segments = typeInfo.MenuPath.Split('/');
            var group = root;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (!string.IsNullOrWhiteSpace(segments[i]))
                {
                    group = group.GetOrAdd(segments[i]);
                }
            }

            group.Items.Add(typeInfo);
        }

        private static void AppendGroup(List<SearchTreeEntry> entries, MenuGroup group, int level)
        {
            group.Groups.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
            group.Items.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
            for (var i = 0; i < group.Groups.Count; i++)
            {
                var child = group.Groups[i];
                entries.Add(new SearchTreeGroupEntry(new GUIContent(child.Name), level + 1));
                AppendGroup(entries, child, level + 1);
            }

            for (var i = 0; i < group.Items.Count; i++)
            {
                var item = group.Items[i];
                entries.Add(new SearchTreeEntry(new GUIContent(item.DisplayName)) { level = level + 1, userData = item.Type });
            }
        }

        private sealed class MenuGroup
        {
            public MenuGroup(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public List<MenuGroup> Groups { get; } = new List<MenuGroup>();
            public List<NodeTypeInfo> Items { get; } = new List<NodeTypeInfo>();

            public MenuGroup GetOrAdd(string name)
            {
                for (var i = 0; i < Groups.Count; i++)
                {
                    if (string.Equals(Groups[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return Groups[i];
                    }
                }

                var group = new MenuGroup(name);
                Groups.Add(group);
                return group;
            }
        }
    }
}
