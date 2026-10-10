using System;
using UnityEditor;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor.Tests
{
    internal sealed class NodeGraphFixtureBuilder : IDisposable
    {
        private readonly string assetPath;

        public NodeGraphFixtureBuilder()
        {
            assetPath = $"Assets/FGUFW/NodeGraph/Tests/Temp-{Guid.NewGuid():N}.asset";
            Graph = NodeGraphAssetFactory.CreateAtPath(assetPath);
        }

        public NodeGraphAsset Graph { get; }

        public T AddNode<T>(Vector2 position) where T : NodeDefinition
        {
            return (T)NodeGraphEditorUtility.CreateNode(Graph, typeof(T), position);
        }

        public void Dispose()
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
    }
}
