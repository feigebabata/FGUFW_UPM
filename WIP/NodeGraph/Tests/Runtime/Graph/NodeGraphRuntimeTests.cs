using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace FGUFW.NodeGraph.Tests
{
    public sealed class NodeGraphRuntimeTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (var i = createdObjects.Count - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(createdObjects[i]);
            }

            createdObjects.Clear();
        }

        [Test]
        public void EndCompletesGraphAndCancelsOtherRunningNodes()
        {
            var graph = CreateGraph(out var start, out var end);
            var holding = CreateNode<HoldingNodeDefinition>();
            graph.AddNode(holding);
            AddEdge(graph, start, StartNodeDefinition.StartPortId, holding, HoldingNodeDefinition.InputPortId);
            AddEdge(graph, start, StartNodeDefinition.StartPortId, end, EndNodeDefinition.EndPortId);

            using (var executor = new NodeGraphExecutor(graph, new NodeBlackboard()))
            {
                executor.Start();

                Assert.That(executor.Status, Is.EqualTo(NodeGraphRuntimeStatus.Completed));
                Assert.That(executor.GetRuntime(holding).State, Is.EqualTo(NodeRuntimeState.Cancelled));
            }
        }

        [Test]
        public void CancelStopsRunningNodes()
        {
            var graph = CreateGraph(out var start, out _);
            var holding = CreateNode<HoldingNodeDefinition>();
            graph.AddNode(holding);
            AddEdge(graph, start, StartNodeDefinition.StartPortId, holding, HoldingNodeDefinition.InputPortId);

            using (var executor = new NodeGraphExecutor(graph, new NodeBlackboard()))
            {
                executor.Start();
                executor.Cancel();

                Assert.That(executor.Status, Is.EqualTo(NodeGraphRuntimeStatus.Cancelled));
                Assert.That(executor.GetRuntime(holding).State, Is.EqualTo(NodeRuntimeState.Cancelled));
            }
        }

        [Test]
        public void MissingPathToEndStallsGraph()
        {
            var graph = CreateGraph(out _, out _);

            using (var executor = new NodeGraphExecutor(graph, new NodeBlackboard()))
            {
                executor.Start();

                Assert.That(executor.Status, Is.EqualTo(NodeGraphRuntimeStatus.Stalled));
            }
        }

        private NodeGraphAsset CreateGraph(out StartNodeDefinition start, out EndNodeDefinition end)
        {
            var graph = CreateNode<NodeGraphAsset>();
            start = CreateNode<StartNodeDefinition>();
            end = CreateNode<EndNodeDefinition>();
            graph.AddNode(start);
            graph.AddNode(end);
            return graph;
        }

        private T CreateNode<T>() where T : ScriptableObject
        {
            var value = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(value);
            return value;
        }

        private static void AddEdge(
            NodeGraphAsset graph,
            NodeDefinition output,
            string outputPortId,
            NodeDefinition input,
            string inputPortId)
        {
            Assert.That(
                graph.TryAddEdge(new NodeEdge(output, outputPortId, input, inputPortId), out var error),
                Is.True,
                error);
        }
    }

    internal sealed class HoldingNodeDefinition : NodeDefinition
    {
        public const string InputPortId = "enter";

        private static readonly IReadOnlyList<NodePortDefinition> PortDefinitions = new[]
        {
            new NodePortDefinition(InputPortId, "Enter", NodePortDirection.Input, typeof(FlowPort))
        };

        public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

        public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
        {
            return new HoldingNodeRuntime(this, executor);
        }
    }

    internal sealed class HoldingNodeRuntime : NodeRuntime
    {
        public HoldingNodeRuntime(HoldingNodeDefinition definition, NodeGraphExecutor executor)
            : base(definition, executor)
        {
        }

        protected override void OnEnter(NodeRuntimeEdge sourceEdge)
        {
        }
    }
}
