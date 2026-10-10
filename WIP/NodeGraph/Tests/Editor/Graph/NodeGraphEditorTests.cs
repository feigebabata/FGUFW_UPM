using System.Linq;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FGUFW.NodeGraph.Editor.Tests
{
    public sealed class NodeGraphEditorTests
    {
        [Test]
        public void TestNodesAreExcludedFromCreateMenu()
        {
            var types = NodeTypeRegistry.GetCreatableNodeTypes();

            Assert.That(
                types.Any(info =>
                    info.Type.Assembly.GetName().Name.EndsWith(".Tests")
                    || info.Type.Assembly.GetName().Name.EndsWith(".TestFixtures")),
                Is.False);
        }

        [Test]
        public void StartAndEndKeepDistinctTitleColors()
        {
            var start = ScriptableObject.CreateInstance<StartNodeDefinition>();
            var end = ScriptableObject.CreateInstance<EndNodeDefinition>();
            try
            {
                var startView = new NodeView(start);
                var endView = new NodeView(end);

                Assert.That(
                    startView.titleContainer.style.backgroundColor.value,
                    Is.EqualTo((Color)new Color32(18, 122, 53, 255)));
                Assert.That(
                    endView.titleContainer.style.backgroundColor.value,
                    Is.EqualTo((Color)new Color32(179, 38, 46, 255)));
            }
            finally
            {
                Object.DestroyImmediate(start);
                Object.DestroyImmediate(end);
            }
        }

        [Test]
        public void DeleteSelectionRemovesNodeAndKeepsRequiredNodeSelected()
        {
            using (var fixture = new NodeGraphFixtureBuilder())
            {
                var graph = fixture.Graph;
                var removable = fixture.AddNode<EditorTestNode>(Vector2.zero);
                var start = graph.GetStartNode();
                Assert.That(
                    graph.TryAddEdge(
                        new NodeEdge(start, StartNodeDefinition.StartPortId, removable, EditorTestNode.InputPortId),
                        out var error),
                    Is.True,
                    error);

                var window = ScriptableObject.CreateInstance<NodeGraphEditorWindow>();
                var view = new NodeGraphView(window);
                try
                {
                    view.SetGraph(graph);
                    var startView = view.nodes.OfType<NodeView>().Single(node => node.Definition == start);
                    var removableView = view.nodes.OfType<NodeView>().Single(node => node.Definition == removable);
                    view.AddToSelection(startView);
                    view.AddToSelection(removableView);

                    view.DeleteSelection("Delete", GraphView.AskUser.DontAskUser);

                    Assert.That(graph.ContainsNode(removable), Is.False);
                    Assert.That(graph.Edges, Is.Empty);
                    Assert.That(
                        view.selection.OfType<NodeView>().Single().Definition,
                        Is.EqualTo(start));
                }
                finally
                {
                    view.Dispose();
                    Object.DestroyImmediate(window);
                }
            }
        }
    }
}
