using NUnit.Framework;
using UnityEngine;

namespace FGUFW.NodeGraph.Tests
{
    public sealed class NodeGraphDataTests
    {
        private NodeGraphAsset graph;
        private StartNodeDefinition start;
        private EndNodeDefinition end;

        [SetUp]
        public void SetUp()
        {
            graph = ScriptableObject.CreateInstance<NodeGraphAsset>();
            start = ScriptableObject.CreateInstance<StartNodeDefinition>();
            end = ScriptableObject.CreateInstance<EndNodeDefinition>();
            graph.AddNode(start);
            graph.AddNode(end);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(start);
            Object.DestroyImmediate(end);
            Object.DestroyImmediate(graph);
        }

        [Test]
        public void ValidEdgeCanBeAddedAndValidated()
        {
            var edge = new NodeEdge(start, StartNodeDefinition.StartPortId, end, EndNodeDefinition.EndPortId);

            Assert.That(graph.TryAddEdge(edge, out var error), Is.True, error);
            Assert.That(NodeGraphValidator.Validate(graph).IsValid, Is.True);
        }

        [Test]
        public void EdgeMutationUsesGraphValidation()
        {
            var edge = new NodeEdge(end, EndNodeDefinition.EndPortId, start, StartNodeDefinition.StartPortId);

            Assert.That(graph.TryValidateEdge(edge, out var validationError), Is.False);
            Assert.That(graph.TryAddEdge(edge, out var mutationError), Is.False);
            Assert.That(mutationError, Is.EqualTo(validationError));
        }
    }
}
