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

        private void Start()
        {
            if (playOnStart)
            {
                StartGraph(new NodeBlackboard());
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

        public void StartGraph(INodeBlackboard blackboard, object userData = null)
        {
            Executor?.Dispose();
            Blackboard = blackboard ?? throw new System.ArgumentNullException(nameof(blackboard));
            Executor = new NodeGraphExecutor(graph, blackboard, this, userData, debugName);
            Executor.Start();
        }
    }
}
