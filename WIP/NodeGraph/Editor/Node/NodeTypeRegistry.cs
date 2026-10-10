using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace FGUFW.NodeGraph.Editor
{
    internal readonly struct NodeTypeInfo
    {
        public NodeTypeInfo(Type type, string menuPath, string displayName)
        {
            Type = type;
            MenuPath = menuPath;
            DisplayName = displayName;
        }

        public Type Type { get; }
        public string MenuPath { get; }
        public string DisplayName { get; }
    }

    internal static class NodeTypeRegistry
    {
        private static IReadOnlyList<NodeTypeInfo> cachedTypes;
        public static IReadOnlyList<NodeTypeInfo> GetCreatableNodeTypes()
        {
            if (cachedTypes != null)
            {
                return cachedTypes;
            }

            cachedTypes = TypeCache.GetTypesDerivedFrom<NodeDefinition>()
                .Where(type => type != null
                    && !type.IsAbstract
                    && !type.IsGenericTypeDefinition
                    && type != typeof(StartNodeDefinition)
                    && type != typeof(EndNodeDefinition)
                    && !IsTestType(type))
                .Select(GetInfo)
                .OrderBy(info => info.MenuPath, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return cachedTypes;
        }

        public static NodeTypeInfo GetInfo(Type type)
        {
            var attribute = (NodeMenuAttribute)Attribute.GetCustomAttribute(type, typeof(NodeMenuAttribute), false);
            var rawPath = attribute?.Path?.Trim('/');
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                rawPath = type.Name;
            }

            var separator = rawPath.LastIndexOf('/');
            var pathName = separator >= 0 ? rawPath.Substring(separator + 1) : rawPath;
            var displayName = string.IsNullOrWhiteSpace(attribute?.DisplayName) ? pathName : attribute.DisplayName;
            return new NodeTypeInfo(type, rawPath, displayName);
        }

        private static bool IsTestType(Type type)
        {
            var assemblyName = type.Assembly.GetName().Name;
            return assemblyName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase)
                || assemblyName.EndsWith(".TestFixtures", StringComparison.OrdinalIgnoreCase);
        }
    }
}
