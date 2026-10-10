using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    internal sealed class NodeGraphView : GraphView
    {
        private const string StyleSheetPath = "Assets/FGUFW/NodeGraph/Editor/Graph/NodeGraph.uss";
        private readonly NodeGraphEditorWindow window;
        private readonly Dictionary<NodeDefinition, NodeView> nodeViews = new Dictionary<NodeDefinition, NodeView>();
        private readonly Dictionary<NodeEdge, Edge> edgeViews = new Dictionary<NodeEdge, Edge>();
        private readonly HashSet<NodeEdge> debugHighlightedEdges = new HashSet<NodeEdge>();
        private readonly NodeSearchProvider searchProvider;
        private readonly NodeEdgeConnectorListener edgeConnectorListener;
        private NodeGraphAsset graph;
        private bool suppressGraphChanges;
        private bool hasPointerPosition;
        private bool hasInitialGeometry;
        private int snapRequestVersion;
        private Vector2 lastPointerPosition;
        public NodeGraphView(NodeGraphEditorWindow window)
        {
            this.window = window;
            style.flexGrow = 1f;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            var grid = new GridBackground();
            grid.AddToClassList("node-graph-grid");
            Insert(0, grid);
            grid.StretchToParentSize();
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            VisualElementExtensions.AddManipulator(this, new ContentDragger());
            VisualElementExtensions.AddManipulator(this, new SelectionDragger());
            VisualElementExtensions.AddManipulator(this, new RectangleSelector());
            edgeConnectorListener = new NodeEdgeConnectorListener(this);
            searchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            searchProvider.hideFlags = HideFlags.HideAndDontSave;
            searchProvider.Initialize(window, this);
            nodeCreationRequest = context =>
            {
                searchProvider.ClearConnectedOutput();
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchProvider);
            };
            graphViewChanged = OnGraphViewChanged;
            serializeGraphElements = SerializeSelectedElements;
            canPasteSerializedData = NodeClipboard.CanPaste;
            unserializeAndPaste = UnserializeAndPaste;
            deleteSelection = DeleteSelection;
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerLeaveEvent>(_ => hasPointerPosition = false);
            RegisterCallback<AttachToPanelEvent>(_ => Snap());
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public NodeGraphAsset Graph => graph;

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports
                .ToList()
                .Where(candidate => candidate != startPort
                    && candidate.node != startPort.node
                    && candidate.direction != startPort.direction
                    && candidate.portType == startPort.portType
                    && !WouldDuplicateEdge(startPort, candidate))
                .ToList();
        }

        public void Dispose()
        {
            if (searchProvider != null)
            {
                UnityEngine.Object.DestroyImmediate(searchProvider);
            }
        }

        public void SetGraph(NodeGraphAsset value)
        {
            graph = value;
            Rebuild();
            Snap();
        }

        public void Snap()
        {
            if (graph == null || !nodes.Any())
            {
                return;
            }

            var requestVersion = ++snapRequestVersion;
            schedule.Execute(() =>
            {
                if (requestVersion == snapRequestVersion
                    && panel != null
                    && graph != null
                    && nodes.Any())
                {
                    FrameAll();
                }
            }).ExecuteLater(10);
        }

        public void Rebuild()
        {
            suppressGraphChanges = true;
            try
            {
                DeleteElements(graphElements.ToList());
                nodeViews.Clear();
                edgeViews.Clear();
                debugHighlightedEdges.Clear();
                if (graph == null)
                {
                    return;
                }

                for (var i = 0; i < graph.Nodes.Count; i++)
                {
                    var definition = graph.Nodes[i];
                    if (definition == null)
                    {
                        continue;
                    }

                    var view = CreateNodeView(definition);
                    AddElement(view);
                }

                for (var i = 0; i < graph.Edges.Count; i++)
                {
                    var edgeData = graph.Edges[i];
                    if (!nodeViews.TryGetValue(edgeData.OutputNode, out var outputView)
                        || !nodeViews.TryGetValue(edgeData.InputNode, out var inputView))
                    {
                        continue;
                    }

                    var outputPort = outputView.GetPort(edgeData.OutputPortId);
                    var inputPort = inputView.GetPort(edgeData.InputPortId);
                    if (outputPort == null || inputPort == null)
                    {
                        continue;
                    }

                    var edgeView = outputPort.ConnectTo(inputPort);
                    edgeViews[edgeData] = edgeView;
                    AddElement(edgeView);
                }
            }
            finally
            {
                suppressGraphChanges = false;
            }
        }

        public NodeView CreateNode(Type nodeType, Vector2 position, Port connectedOutput = null)
        {
            if (graph == null)
            {
                window.ShowMessage("Select a NodeGraphAsset first.");
                return null;
            }

            try
            {
                var definition = NodeGraphEditorUtility.CreateNode(graph, nodeType, position);
                var view = CreateNodeView(definition);
                AddElement(view);
                ClearSelection();
                AddToSelection(view);

                if (connectedOutput != null)
                {
                    var input = view.GetFirstCompatibleInput(connectedOutput.portType);
                    if (input != null)
                    {
                        ConnectPorts(connectedOutput, input);
                    }
                }

                window.RefreshValidation();
                return view;
            }
            catch (Exception exception)
            {
                window.ShowMessage(exception.Message);
                return null;
            }
        }

        public void OpenNodeSearch(Port connectedOutput, Vector2 panelPosition)
        {
            var graphPosition = VisualElementExtensions.WorldToLocal(contentViewContainer, panelPosition);
            var screenPosition = window.position.position + panelPosition;
            searchProvider.SetConnectedOutput(connectedOutput, graphPosition);
            SearchWindow.Open(new SearchWindowContext(screenPosition), searchProvider);
        }

        public bool AddEdgeFromConnector(Edge edge)
        {
            if (edge == null || edge.output == null || edge.input == null)
            {
                return false;
            }

            if (!TryCreateEdgeData(edge, out var error))
            {
                window.ShowMessage(error);
                return false;
            }

            edge.output.Connect(edge);
            edge.input.Connect(edge);
            AddElement(edge);
            window.RefreshValidation();
            return true;
        }

        public bool ConnectPorts(Port output, Port input)
        {
            if (output == null || input == null)
            {
                return false;
            }

            return AddEdgeFromConnector(new Edge
            {
                output = output,
                input = input
            });
        }

        public void RefreshRuntimeDebug(NodeGraphDebugObserver observer)
        {
            foreach (var view in nodeViews.Values)
            {
                view.ClearDebugState();
            }

            var activeEdges = new HashSet<NodeEdge>();
            if (observer != null && observer.Executor.Graph == graph)
            {
                foreach (var pair in edgeViews)
                {
                    if (observer.WasEdgeTriggeredRecently(pair.Key, 0.5d))
                    {
                        activeEdges.Add(pair.Key);
                    }
                }
            }

            foreach (var edgeData in debugHighlightedEdges.ToArray())
            {
                if (!edgeViews.TryGetValue(edgeData, out var edge)
                    || !activeEdges.Contains(edgeData)
                    || edge.selected)
                {
                    if (edge != null)
                    {
                        ResetEdgeDebugColor(edge);
                    }

                    debugHighlightedEdges.Remove(edgeData);
                }
            }

            if (observer == null || observer.Executor.Graph != graph)
            {
                return;
            }

            foreach (var pair in nodeViews)
            {
                if (observer.TryGetNodeState(pair.Key, out var state))
                {
                    pair.Value.SetDebugState(state);
                }
            }

            foreach (var edgeData in activeEdges)
            {
                var edge = edgeViews[edgeData];
                if (edge.selected)
                {
                    continue;
                }

                SetEdgeDebugColor(edge, new Color32(255, 184, 55, 255));
                debugHighlightedEdges.Add(edgeData);
            }
        }

        private NodeView CreateNodeView(NodeDefinition definition)
        {
            var view = new NodeView(definition, edgeConnectorListener);
            nodeViews.Add(definition, view);
            return view;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (suppressGraphChanges || graph == null)
            {
                return change;
            }

            if (change.edgesToCreate != null)
            {
                var acceptedEdges = new List<Edge>();
                for (var i = 0; i < change.edgesToCreate.Count; i++)
                {
                    var edge = change.edgesToCreate[i];
                    if (TryCreateEdgeData(edge, out _))
                    {
                        acceptedEdges.Add(edge);
                    }
                }

                change.edgesToCreate = acceptedEdges;
            }

            if (change.elementsToRemove != null)
            {
                for (var i = 0; i < change.elementsToRemove.Count; i++)
                {
                    if (change.elementsToRemove[i] is Edge edge && TryGetEdgeData(edge, out var edgeData))
                    {
                        graph.RemoveEdge(edgeData);
                        edgeViews.Remove(edgeData);
                        debugHighlightedEdges.Remove(edgeData);
                    }
                }
            }

            if (change.movedElements != null)
            {
                for (var i = 0; i < change.movedElements.Count; i++)
                {
                    if (!(change.movedElements[i] is NodeView nodeView))
                    {
                        continue;
                    }

                    nodeView.Definition.SetPosition(nodeView.GetPosition().position);
                    EditorUtility.SetDirty(nodeView.Definition);
                }
            }

            EditorUtility.SetDirty(graph);
            window.RefreshValidation();
            return change;
        }

        private bool TryCreateEdgeData(Edge edge, out string error)
        {
            if (!TryGetEdgeData(edge, out var edgeData))
            {
                error = "The edge endpoints are invalid.";
                return false;
            }

            if (!graph.TryAddEdge(edgeData, out error))
            {
                return false;
            }

            edgeViews[edgeData] = edge;
            EditorUtility.SetDirty(graph);
            return true;
        }

        private bool TryGetEdgeData(Edge edge, out NodeEdge edgeData)
        {
            var outputNode = edge?.output?.node as NodeView;
            var inputNode = edge?.input?.node as NodeView;
            var outputPortId = outputNode?.GetPortId(edge.output);
            var inputPortId = inputNode?.GetPortId(edge.input);
            if (outputNode == null
                || inputNode == null
                || string.IsNullOrWhiteSpace(outputPortId)
                || string.IsNullOrWhiteSpace(inputPortId))
            {
                edgeData = default;
                return false;
            }

            edgeData = new NodeEdge(outputNode.Definition, outputPortId, inputNode.Definition, inputPortId);
            return true;
        }

        private bool WouldDuplicateEdge(Port first, Port second)
        {
            if (graph == null)
            {
                return false;
            }

            var output = first.direction == Direction.Output ? first : second;
            var input = first.direction == Direction.Input ? first : second;
            var outputNode = output.node as NodeView;
            var inputNode = input.node as NodeView;
            if (outputNode == null || inputNode == null)
            {
                return false;
            }

            return graph.ContainsEdge(new NodeEdge(
                outputNode.Definition,
                outputNode.GetPortId(output),
                inputNode.Definition,
                inputNode.GetPortId(input)));
        }

        private string SerializeSelectedElements(IEnumerable<GraphElement> elements)
        {
            if (graph == null)
            {
                return string.Empty;
            }

            var definitions = elements.OfType<NodeView>().Select(view => view.Definition).ToArray();
            var selectedEdges = new List<NodeEdge>();
            foreach (var edge in elements.OfType<Edge>())
            {
                if (TryGetEdgeData(edge, out var edgeData))
                {
                    selectedEdges.Add(edgeData);
                }
            }

            return NodeClipboard.Serialize(graph, definitions, selectedEdges);
        }

        private void UnserializeAndPaste(string operationName, string serializedData)
        {
            if (graph == null)
            {
                return;
            }

            var isDuplicate = !string.IsNullOrWhiteSpace(operationName)
                && operationName.IndexOf("Duplicate", StringComparison.OrdinalIgnoreCase) >= 0;
            Vector2 anchor;
            if (isDuplicate && NodeClipboard.TryGetSourceCenter(serializedData, out var sourceCenter))
            {
                anchor = sourceCenter + new Vector2(30f, 30f);
            }
            else
            {
                anchor = GetPastePosition();
            }

            if (!NodeClipboard.TryPaste(graph, serializedData, anchor, out var pastedNodes, out var error))
            {
                window.ShowMessage(error);
                return;
            }

            Rebuild();
            ClearSelection();
            for (var i = 0; i < pastedNodes.Count; i++)
            {
                if (nodeViews.TryGetValue(pastedNodes[i], out var view))
                {
                    AddToSelection(view);
                }
            }

            window.RefreshValidation();
        }

        internal void DeleteSelection(string operationName, AskUser askUser)
        {
            if (graph == null || selection.Count == 0)
            {
                return;
            }

            var protectedNodes = selection
                .OfType<NodeView>()
                .Where(view => view.Definition is StartNodeDefinition
                    || view.Definition is EndNodeDefinition)
                .Select(view => view.Definition)
                .ToArray();
            var nodesToDelete = selection
                .OfType<NodeView>()
                .Where(view => !(view.Definition is StartNodeDefinition)
                    && !(view.Definition is EndNodeDefinition))
                .Select(view => view.Definition)
                .ToArray();

            foreach (var edge in selection.OfType<Edge>())
            {
                if (TryGetEdgeData(edge, out var edgeData))
                {
                    graph.RemoveEdge(edgeData);
                }
            }

            for (var i = 0; i < nodesToDelete.Length; i++)
            {
                NodeGraphEditorUtility.DeleteNode(graph, nodesToDelete[i]);
            }

            Rebuild();
            ClearSelection();
            for (var i = 0; i < protectedNodes.Length; i++)
            {
                if (nodeViews.TryGetValue(protectedNodes[i], out var view))
                {
                    AddToSelection(view);
                }
            }

            EditorUtility.SetDirty(graph);
            window.RefreshValidation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            lastPointerPosition = VisualElementExtensions.WorldToLocal(contentViewContainer, evt.position);
            hasPointerPosition = true;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (hasInitialGeometry || evt.newRect.width <= 0f || evt.newRect.height <= 0f)
            {
                return;
            }

            hasInitialGeometry = true;
            Snap();
        }

        private Vector2 GetPastePosition()
        {
            if (hasPointerPosition)
            {
                return lastPointerPosition;
            }

            var worldCenter = VisualElementExtensions.LocalToWorld(this, contentRect.center);
            return VisualElementExtensions.WorldToLocal(contentViewContainer, worldCenter);
        }

        private static void SetEdgeDebugColor(Edge edge, Color color)
        {
            if (edge?.edgeControl == null)
            {
                return;
            }

            edge.edgeControl.inputColor = color;
            edge.edgeControl.outputColor = color;
            edge.edgeControl.MarkDirtyRepaint();
        }

        private static void ResetEdgeDebugColor(Edge edge)
        {
            if (edge?.edgeControl == null || edge.input == null || edge.output == null)
            {
                return;
            }

            edge.edgeControl.inputColor = edge.input.portColor;
            edge.edgeControl.outputColor = edge.output.portColor;
            edge.edgeControl.MarkDirtyRepaint();
        }
    }
}
