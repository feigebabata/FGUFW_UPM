using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FGUFW.NodeGraph.Editor
{
    public sealed class NodeGraphEditorWindow : EditorWindow
    {
        [SerializeField]
        private NodeGraphAsset selectedGraph;

        [SerializeField]
        private NodeGraphRunner selectedRunner;

        private ObjectField graphField;
        private ObjectField runnerField;
        private Label validationLabel;
        private Label runtimeStatusLabel;
        private ScrollView blackboardView;
        private VisualElement blackboardPanel;
        private NodeGraphView graphView;
        private int lastObserverRevision = -1;

        [MenuItem("Window/FGUFW/Node Graph")]
        public static void Open()
        {
            GetWindow<NodeGraphEditorWindow>("Node Graph");
        }

        public static void Open(NodeGraphAsset graph)
        {
            var window = GetWindow<NodeGraphEditorWindow>("Node Graph");
            window.SetGraph(graph, true);
            window.Focus();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            var graph = EditorUtility.InstanceIDToObject(instanceId) as NodeGraphAsset;
            if (graph == null)
            {
                return false;
            }

            Open(graph);
            return true;
        }

        public void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            var toolbar = new Toolbar();
            graphField = new ObjectField
            {
                objectType = typeof(NodeGraphAsset),
                allowSceneObjects = false,
                label = "Graph"
            };
            graphField.style.minWidth = 300f;
            graphField.RegisterValueChangedCallback(evt => SetGraph(evt.newValue as NodeGraphAsset, true));
            toolbar.Add(graphField);

            runnerField = new ObjectField
            {
                objectType = typeof(NodeGraphRunner),
                allowSceneObjects = true,
                label = "Runner"
            };
            runnerField.style.minWidth = 250f;
            runnerField.RegisterValueChangedCallback(evt => SetRunner(evt.newValue as NodeGraphRunner));
            toolbar.Add(runnerField);

            toolbar.Add(new ToolbarButton(Snap)
            {
                text = "Snap",
                tooltip = "居中整个图，内容过大时自动调整缩放。"
            });
            toolbar.Add(new ToolbarButton(Save) { text = "Save" });
            toolbar.Add(new ToolbarButton(ShowValidation) { text = "Validate" });

            validationLabel = new Label();
            validationLabel.style.marginLeft = 8f;
            toolbar.Add(validationLabel);

            runtimeStatusLabel = new Label();
            runtimeStatusLabel.style.marginLeft = 8f;
            toolbar.Add(runtimeStatusLabel);
            rootVisualElement.Add(toolbar);

            var content = new VisualElement();
            content.style.flexDirection = FlexDirection.Row;
            content.style.flexGrow = 1f;

            graphView = new NodeGraphView(this);
            content.Add(graphView);

            blackboardPanel = CreateBlackboardPanel();
            content.Add(blackboardPanel);
            rootVisualElement.Add(content);

            Selection.selectionChanged += OnUnitySelectionChanged;
            NodeGraphDebugRegistry.Changed += OnDebugRegistryChanged;
            EditorApplication.update += OnEditorUpdate;

            if (selectedRunner != null)
            {
                SetRunner(selectedRunner);
            }
            else
            {
                SetGraph(selectedGraph, false);
            }

            SyncFromUnitySelection();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= OnUnitySelectionChanged;
            NodeGraphDebugRegistry.Changed -= OnDebugRegistryChanged;
            EditorApplication.update -= OnEditorUpdate;
            graphView?.Dispose();
        }

        internal static NodeGraphRunner ResolveRunner(UnityEngine.Object selectedObject)
        {
            if (selectedObject is NodeGraphRunner runner)
            {
                return runner;
            }

            if (selectedObject is GameObject gameObject)
            {
                return gameObject.GetComponent<NodeGraphRunner>();
            }

            if (selectedObject is Component component)
            {
                return component.GetComponent<NodeGraphRunner>();
            }

            return null;
        }

        internal static NodeGraphAsset ResolveGraph(UnityEngine.Object selectedObject)
        {
            if (selectedObject is NodeGraphAsset graph)
            {
                return graph;
            }

            if (selectedObject is NodeDefinition node)
            {
                var path = AssetDatabase.GetAssetPath(node);
                return AssetDatabase.LoadAssetAtPath<NodeGraphAsset>(path);
            }

            return null;
        }

        internal void RefreshValidation()
        {
            if (validationLabel == null)
            {
                return;
            }

            if (selectedGraph == null)
            {
                validationLabel.text = "No graph";
                validationLabel.tooltip = string.Empty;
                return;
            }

            var result = NodeGraphEditorUtility.Validate(selectedGraph);
            validationLabel.text = result.IsValid ? "Valid" : $"{result.Errors.Count} error(s)";
            validationLabel.tooltip = result.ErrorMessage;
        }

        internal void ShowMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                ShowNotification(new GUIContent(message));
            }
        }

        private void SetGraph(NodeGraphAsset graph, bool clearRunner)
        {
            selectedGraph = graph;
            lastObserverRevision = -1;

            if (clearRunner)
            {
                selectedRunner = null;
                runnerField?.SetValueWithoutNotify(null);
            }

            if (graphField != null && graphField.value != graph)
            {
                graphField.SetValueWithoutNotify(graph);
            }

            graphView?.SetGraph(graph);
            RefreshValidation();
            RefreshRuntimeDisplay();
        }

        private void SetRunner(NodeGraphRunner runner)
        {
            selectedRunner = runner;
            lastObserverRevision = -1;
            runnerField?.SetValueWithoutNotify(runner);
            SetGraph(runner == null ? selectedGraph : runner.Graph, false);
        }

        private void Save()
        {
            AssetDatabase.SaveAssets();
            RefreshValidation();
        }

        private void Snap()
        {
            graphView?.Snap();
        }

        private void ShowValidation()
        {
            if (selectedGraph == null)
            {
                EditorUtility.DisplayDialog("Node Graph", "No graph is selected.", "OK");
                return;
            }

            var result = NodeGraphEditorUtility.Validate(selectedGraph);
            var title = result.IsValid ? "Graph Valid" : "Graph Validation Failed";
            var message = result.IsValid ? "No validation errors." : result.ErrorMessage;
            EditorUtility.DisplayDialog(title, message, "OK");
            RefreshValidation();
        }

        private VisualElement CreateBlackboardPanel()
        {
            var panel = new VisualElement();
            panel.style.width = 280f;
            panel.style.paddingLeft = 8f;
            panel.style.paddingRight = 8f;
            panel.style.borderLeftWidth = 1f;
            panel.style.borderLeftColor = (Color)new Color32(55, 55, 55, 255);
            panel.style.display = DisplayStyle.None;

            var title = new Label("Runtime Blackboard");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginTop = 6f;
            title.style.marginBottom = 6f;
            panel.Add(title);

            blackboardView = new ScrollView();
            blackboardView.style.flexGrow = 1f;
            panel.Add(blackboardView);
            return panel;
        }

        private void OnUnitySelectionChanged()
        {
            SyncFromUnitySelection();
        }

        private void SyncFromUnitySelection()
        {
            var selectedObject = Selection.activeObject;
            var runner = ResolveRunner(selectedObject);
            if (runner != null)
            {
                SetRunner(runner);
                return;
            }

            var graph = ResolveGraph(selectedObject);
            if (graph != null)
            {
                SetGraph(graph, true);
            }
        }

        private void OnDebugRegistryChanged()
        {
            lastObserverRevision = -1;
            RefreshRuntimeDisplay();
        }

        private void OnEditorUpdate()
        {
            RefreshRuntimeDisplay();
        }

        private NodeGraphDebugObserver GetSelectedObserver()
        {
            return selectedRunner == null
                ? null
                : NodeGraphDebugRegistry.GetObserver(selectedRunner.Executor);
        }

        private void RefreshRuntimeDisplay()
        {
            var observer = GetSelectedObserver();
            graphView?.RefreshRuntimeDebug(observer);

            if (runtimeStatusLabel != null)
            {
                runtimeStatusLabel.text = selectedRunner == null
                    ? string.Empty
                    : selectedRunner.Executor == null
                        ? "Not Running"
                        : selectedRunner.Executor.Status.ToString();
            }

            if (blackboardPanel == null)
            {
                return;
            }

            blackboardPanel.style.display = observer == null ? DisplayStyle.None : DisplayStyle.Flex;
            if (observer == null || observer.Revision == lastObserverRevision)
            {
                return;
            }

            lastObserverRevision = observer.Revision;
            RefreshBlackboard(observer.Executor.Blackboard);
        }

        private void RefreshBlackboard(INodeBlackboard blackboard)
        {
            blackboardView.Clear();
            foreach (var entry in blackboard.Entries.OrderBy(entry => entry.Key))
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 2f;

                var keyLabel = new Label(entry.Key);
                keyLabel.style.minWidth = 100f;
                keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(keyLabel);

                var valueLabel = new Label(FormatBlackboardValue(entry.Value));
                valueLabel.style.flexGrow = 1f;
                valueLabel.tooltip = entry.Value?.GetType().FullName ?? "null";
                row.Add(valueLabel);
                blackboardView.Add(row);
            }
        }

        private static string FormatBlackboardValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is UnityEngine.Object unityObject)
            {
                return unityObject == null ? "null" : unityObject.name;
            }

            return value.ToString();
        }
    }
}
