using UnityEngine;

namespace FGUFW.NodeGraph
{
    public sealed class NodeGraphRunner : MonoBehaviour
    {
        [SerializeField]
        private NodeGraphAsset graph;
        [SerializeField]
        private bool playOnStart;
        [SerializeField]
        private bool useUnscaledTime;
        [SerializeField]
        private string debugName;
        public NodeGraphAsset Graph => graph;
        public NodeGraphExecutor Executor { get; private set; }
        public INodeBlackboard Blackboard { get; private set; }
        public NodeBlackboardComponent BlackboardComponent { get; private set; }

        private void Start()
        {
            if (playOnStart)
            {
                StartGraph();
            }
        }

        private void Update()
        {
            if (Executor == null)
            {
                return;
            }

            Executor.Tick(useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void OnDisable()
        {
            Executor?.Cancel();
        }

        private void OnDestroy()
        {
            Executor?.Dispose();
        }

        public void StartGraph(INodeBlackboard blackboard = null, object userData = null)
        {
            Executor?.Dispose();
            Blackboard = blackboard ?? new NodeBlackboard();
            BlackboardComponent = GetComponent<NodeBlackboardComponent>();
            BlackboardComponent?.CopyTo(Blackboard);
            Executor = new NodeGraphExecutor(graph, Blackboard, this, userData, debugName);
            Executor.Start();
        }
    }
}
