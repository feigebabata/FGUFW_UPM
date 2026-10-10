# NodeGraph Plan 2.2：Runtime 与调试

## 1. 目标

在 `plan2` 编辑器和数据结构基础上补充运行时：

- 每个 NodeDefinition 对应一个 NodeRuntime。
- NodeRuntime 持有入口和出口 NodeRuntimeEdge。
- 当前 NodeRuntime 完成后主动推进出口节点。
- 使用调度队列执行后继节点，避免递归调用栈溢出。
- Start 节点负责启动 Graph。
- End 节点一进入就结束整个 Graph。
- End 不自动等待其他分支。
- 多个分支需要汇合时使用专用 Wait All 节点。
- Progress 节点可以跨帧 Tick 并上报进度。
- Blackboard 是纯运行时对象，与 NodeGraphAsset 无关。
- Blackboard 在创建 Executor 时由外部传入。
- Editor 可以在 Play Mode 查看节点、Edge、进度和 Blackboard。
- Runtime 不引用 UnityEditor。

## 2. 核心语义

运行时不采用“所有活动分支自然归零后完成 Graph”的规则。

Graph 只有两种结束方式：

```text
任意执行路径进入 End
    → Graph Completed

外部调用 Cancel 或出现错误
    → Graph Cancelled / Failed
```

如果有多个并行分支：

```text
Start
├── A ─────→ End
└── B ─────→ ...
```

A 先进入 End 后：

- 整个 Graph 立即完成。
- 待执行 Edge 队列被清空。
- B 等仍在运行的节点被取消。

如果要求 A、B 都完成后才能继续：

```text
Start
├── A ─┐
│      ├── Wait ── End
└── B ─┘
```

同步行为由 Wait 明确表达。

## 3. Runtime 目录

```text
Runtime/
├── Graph/
│   ├── NodeGraphAsset.cs
│   ├── NodeEdge.cs
│   ├── NodeGraphValidation.cs
│   ├── NodeGraphExecutor.cs
│   ├── NodeExecutionContext.cs
│   ├── NodeRuntimeEdge.cs
│   ├── NodeGraphRuntimeStatus.cs
│   └── NodeGraphRunner.cs
├── Node/
│   ├── NodeDefinition.cs
│   ├── ProgressNodeDefinition.cs
│   ├── NodeRuntime.cs
│   ├── NodeRuntimeState.cs
│   ├── NodePortDefinition.cs
│   └── BuiltIn/
│       ├── StartNodeDefinition.cs
│       ├── StartNodeRuntime.cs
│       ├── EndNodeDefinition.cs
│       ├── EndNodeRuntime.cs
│       ├── WaitAllNodeDefinition.cs
│       ├── WaitAllNodeRuntime.cs
│       ├── DelayNodeDefinition.cs
│       ├── DelayNodeRuntime.cs
│       ├── LogNodeDefinition.cs
│       └── LogNodeRuntime.cs
├── Blackboard/
│   ├── INodeBlackboard.cs
│   └── NodeBlackboard.cs
├── Debugging/
│   └── INodeGraphObserver.cs
```

## 4. NodeDefinition 创建 Runtime

NodeDefinition 继续保存静态配置，但增加 Runtime 工厂：

```csharp
public abstract class NodeDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Vector2 position;

    public Vector2 Position => position;

    public abstract IReadOnlyList<NodePortDefinition> Ports { get; }

    public abstract NodeRuntime CreateRuntime(NodeGraphExecutor executor);
}
```

外部节点同时提供：

- 配置字段。
- Port 定义。
- Runtime 创建方法。

示例：

```csharp
[NodeMenu("Gameplay/Damage", "Damage")]
public sealed class DamageNodeDefinition : NodeDefinition
{
    [SerializeField]
    private string targetKey = "Target";

    [SerializeField]
    private string damageKey = "Damage";

    public string TargetKey => targetKey;

    public string DamageKey => damageKey;

    public override IReadOnlyList<NodePortDefinition> Ports => PortDefinitions;

    public override NodeRuntime CreateRuntime(NodeGraphExecutor executor)
    {
        return new DamageNodeRuntime(this, executor);
    }
}
```

NodeDefinition 仍然不能保存运行状态。

## 5. Runtime Graph 构建

Executor 初始化采用两阶段。

第一阶段为每个 Definition 创建 Runtime：

```csharp
private readonly Dictionary<NodeDefinition, NodeRuntime> runtimes = new();

private void CreateRuntimes()
{
    foreach (var definition in Graph.Nodes)
    {
        var runtime = definition.CreateRuntime(this);

        if (runtime == null)
        {
            throw new InvalidOperationException(
                $"Node '{definition.name}' returned a null runtime.");
        }

        runtimes.Add(definition, runtime);
    }
}
```

第二阶段根据 NodeEdge 创建 NodeRuntimeEdge：

```csharp
private void CreateRuntimeEdges()
{
    foreach (var edge in Graph.Edges)
    {
        var outputNode = runtimes[edge.OutputNode];
        var inputNode = runtimes[edge.InputNode];

        var runtimeEdge = new NodeRuntimeEdge(
            outputNode,
            edge.OutputPortId,
            inputNode,
            edge.InputPortId);

        outputNode.AddOutput(runtimeEdge);
        inputNode.AddInput(runtimeEdge);
        runtimeEdges.Add(runtimeEdge);
    }
}
```

构建完成后，运行时不再遍历 NodeGraphAsset 的 Edge 查找后继节点。

## 6. NodeRuntimeEdge

不能只保存 `List<NodeRuntime>`，因为 Wait 和 Editor 调试都需要知道具体端口和具体 Edge。

```csharp
public sealed class NodeRuntimeEdge
{
    public NodeRuntimeEdge(
        NodeRuntime outputNode,
        string outputPortId,
        NodeRuntime inputNode,
        string inputPortId)
    {
        OutputNode = outputNode;
        OutputPortId = outputPortId;
        InputNode = inputNode;
        InputPortId = inputPortId;
    }

    public NodeRuntime OutputNode { get; }

    public string OutputPortId { get; }

    public NodeRuntime InputNode { get; }

    public string InputPortId { get; }
}
```

Editor 调试时可以通过：

```csharp
runtimeEdge.OutputNode.Definition
runtimeEdge.InputNode.Definition
```

找到对应 NodeView 和 GraphView Edge。

## 7. NodeRuntime 状态

```csharp
public enum NodeRuntimeState
{
    Idle,
    Running,
    Completed,
    Failed,
    Cancelled
}
```

基础 Runtime：

```csharp
public abstract class NodeRuntime
{
    private readonly List<NodeRuntimeEdge> inputEdges = new();
    private readonly List<NodeRuntimeEdge> outputEdges = new();

    protected NodeRuntime(NodeDefinition definition, NodeGraphExecutor executor)
    {
        Definition = definition;
        Executor = executor;
    }

    public NodeDefinition Definition { get; }

    public NodeGraphExecutor Executor { get; }

    public NodeExecutionContext Context => Executor.Context;

    public IReadOnlyList<NodeRuntimeEdge> InputEdges => inputEdges;

    public IReadOnlyList<NodeRuntimeEdge> OutputEdges => outputEdges;

    public NodeRuntimeState State { get; private set; }

    public virtual float Progress => -1f;

    protected virtual bool AcceptEnterWhileRunning => false;
}
```

RuntimeEdge 只能由 Executor 构建：

```csharp
internal void AddInput(NodeRuntimeEdge edge)
{
    inputEdges.Add(edge);
}

internal void AddOutput(NodeRuntimeEdge edge)
{
    outputEdges.Add(edge);
}
```

## 8. Enter

```csharp
internal void Enter(NodeRuntimeEdge sourceEdge)
{
    if (Executor.Status != NodeGraphRuntimeStatus.Running)
    {
        return;
    }

    if (State == NodeRuntimeState.Running && !AcceptEnterWhileRunning)
    {
        Executor.FailGraph(
            $"Node '{Definition.name}' was entered while already running.");
        return;
    }

    if (State != NodeRuntimeState.Running)
    {
        ResetForEnter();
        State = NodeRuntimeState.Running;
        Executor.AddRunningNode(this);
    }

    Executor.NotifyNodeEntered(this, sourceEdge);
    OnEnter(sourceEdge);
}
```

节点实现：

```csharp
protected abstract void OnEnter(NodeRuntimeEdge sourceEdge);
```

重新进入前的清理：

```csharp
protected virtual void ResetForEnter()
{
}
```

普通节点在 Running 状态下再次 Enter 会使 Graph 失败。

需要接受多个入口的节点，例如 Wait，可以重写：

```csharp
protected override bool AcceptEnterWhileRunning => true;
```

## 9. 短节点

短节点在 `OnEnter()` 中直接完成：

```csharp
protected override void OnEnter(NodeRuntimeEdge sourceEdge)
{
    ExecuteLogic();
    Complete("next");
}
```

短节点仍然通过 Executor 队列推进后继节点，不直接递归调用 C# 方法。

## 10. Progress 节点

Progress 节点进入后保持 Running：

```csharp
protected override void OnEnter(NodeRuntimeEdge sourceEdge)
{
    elapsed = 0f;
}
```

每帧 Tick：

```csharp
public override void Tick(float deltaTime)
{
    elapsed += deltaTime;

    Executor.NotifyNodeProgress(this, Progress);

    if (elapsed >= duration)
    {
        Complete("complete");
    }
}
```

进度：

```csharp
public override float Progress =>
    duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
```

`ProgressNodeDefinition` 只负责让 Editor 创建进度条。是否跨帧执行仍由对应 NodeRuntime 决定。

## 11. Complete

```csharp
protected void Complete(params string[] outputPortIds)
{
    if (State != NodeRuntimeState.Running)
    {
        return;
    }

    State = NodeRuntimeState.Completed;
    Executor.RemoveRunningNode(this);
    Executor.NotifyNodeCompleted(this);

    foreach (var edge in OutputEdges)
    {
        if (outputPortIds.Contains(edge.OutputPortId))
        {
            Executor.Enqueue(edge);
        }
    }
}
```

节点可以一次触发多个输出端口：

```csharp
Complete("success", "notify");
```

同一个输出端口可以连接多个 RuntimeEdge，因此每条 Edge 都会进入调度队列。

## 12. 调度队列

NodeRuntime 在完成时负责选择出口，Executor 负责实际调用后继节点。

```csharp
private readonly Queue<NodeRuntimeEdge> pendingEdges = new();

internal void Enqueue(NodeRuntimeEdge edge)
{
    pendingEdges.Enqueue(edge);
}
```

```csharp
private void ProcessPendingEdges()
{
    var dispatchCount = 0;

    while (pendingEdges.Count > 0
        && Status == NodeGraphRuntimeStatus.Running)
    {
        if (++dispatchCount > MaxDispatchPerTick)
        {
            FailGraph("Too many immediate node transitions.");
            return;
        }

        var edge = pendingEdges.Dequeue();

        NotifyEdgeTriggered(edge);
        edge.InputNode.Enter(edge);
    }
}
```

使用队列的原因：

- 避免长链递归导致调用栈过深。
- 避免立即循环造成 StackOverflow。
- 统一捕获异常。
- 统一发送 Edge 调试事件。
- 可以实现单步调试。

默认：

```csharp
public int MaxDispatchPerTick { get; set; } = 1024;
```

## 13. Executor 状态

```csharp
public enum NodeGraphRuntimeStatus
{
    Idle,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled,
    Stalled
}
```

Executor 主要字段：

```csharp
public sealed class NodeGraphExecutor
{
    private readonly Dictionary<NodeDefinition, NodeRuntime> runtimes = new();
    private readonly List<NodeRuntimeEdge> runtimeEdges = new();
    private readonly HashSet<NodeRuntime> runningNodes = new();
    private readonly Queue<NodeRuntimeEdge> pendingEdges = new();
    private readonly List<INodeGraphObserver> observers = new();

    public NodeGraphAsset Graph { get; }

    public INodeBlackboard Blackboard { get; }

    public NodeExecutionContext Context { get; }

    public NodeGraphRuntimeStatus Status { get; private set; }
}
```

## 14. Executor 构造

Blackboard 由外部创建并注入：

```csharp
public NodeGraphExecutor(
    NodeGraphAsset graph,
    INodeBlackboard blackboard,
    UnityEngine.Object owner = null,
    object userData = null)
{
    Graph = graph ?? throw new ArgumentNullException(nameof(graph));
    Blackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
    Context = new NodeExecutionContext(this, blackboard, owner, userData);

    ValidateGraph();
    CreateRuntimes();
    CreateRuntimeEdges();
}
```

Executor 不负责：

- 创建 Blackboard。
- 清空 Blackboard。
- 写入 Blackboard 初始值。
- 决定 Blackboard 是否共享。

## 15. Start

```csharp
public void Start()
{
    if (Status == NodeGraphRuntimeStatus.Running)
    {
        throw new InvalidOperationException("Graph is already running.");
    }

    ResetRuntimes();
    pendingEdges.Clear();
    runningNodes.Clear();

    Status = NodeGraphRuntimeStatus.Running;
    NotifyGraphStarted();

    var startDefinition = Graph.GetStartNode();
    var startRuntime = runtimes[startDefinition];

    startRuntime.Enter(null);
    ProcessPendingEdges();
    DetectStall();
}
```

Start Runtime：

```csharp
public sealed class StartNodeRuntime : NodeRuntime
{
    protected override void OnEnter(NodeRuntimeEdge sourceEdge)
    {
        Complete(StartNodeDefinition.StartPortId);
    }
}
```

## 16. Tick

```csharp
public void Tick(float deltaTime)
{
    if (Status != NodeGraphRuntimeStatus.Running)
    {
        return;
    }

    var snapshot = runningNodes.ToArray();

    foreach (var runtime in snapshot)
    {
        if (runtime.State == NodeRuntimeState.Running)
        {
            runtime.Tick(deltaTime);
        }
    }

    ProcessPendingEdges();
    DetectStall();
}
```

NodeRuntime 默认不需要 Tick：

```csharp
public virtual void Tick(float deltaTime)
{
}
```

## 17. End

End 第一次进入就结束整个 Graph：

```csharp
public sealed class EndNodeRuntime : NodeRuntime
{
    protected override void OnEnter(NodeRuntimeEdge sourceEdge)
    {
        CompleteNodeOnly();
        Executor.CompleteGraph(this);
    }
}
```

```csharp
internal void CompleteGraph(NodeRuntime endRuntime)
{
    if (Status != NodeGraphRuntimeStatus.Running)
    {
        return;
    }

    Status = NodeGraphRuntimeStatus.Completed;
    pendingEdges.Clear();

    foreach (var runtime in runningNodes.ToArray())
    {
        if (runtime != endRuntime)
        {
            runtime.Cancel();
        }
    }

    runningNodes.Clear();
    NotifyGraphCompleted(endRuntime);
}
```

End 不等待其他分支。

## 18. Wait All 节点

Wait All Runtime 根据进入它的 RuntimeEdge 判断哪些分支已经到达：

```csharp
public sealed class WaitAllNodeRuntime : NodeRuntime
{
    private readonly HashSet<NodeRuntimeEdge> arrivedEdges = new();

    protected override bool AcceptEnterWhileRunning => true;

    protected override void ResetForEnter()
    {
        arrivedEdges.Clear();
    }

    protected override void OnEnter(NodeRuntimeEdge sourceEdge)
    {
        if (sourceEdge == null)
        {
            Executor.FailGraph("Wait cannot be entered without a source edge.");
            return;
        }

        arrivedEdges.Add(sourceEdge);
        Executor.NotifyWaitProgress(this, arrivedEdges.Count, InputEdges.Count);

        if (arrivedEdges.Count >= InputEdges.Count)
        {
            Complete("complete");
        }
    }
}
```

第一版 Wait 语义：

```text
等待所有连接到它的入口 Edge 都至少触发一次
```

限制：

- 如果条件分支导致某条入口 Edge 永远不触发，Wait 会一直等待。
- 同一 Edge 重复触发不会重复计数。
- 循环再次进入 Wait 时会在新一轮执行中清空计数。

后续可以增加：

```text
WaitAll
WaitAny
WaitCount
WaitNamedInputs
```

## 19. Fail

第一版节点失败直接结束整个 Graph：

```csharp
protected void Fail(string message, Exception exception = null)
{
    State = NodeRuntimeState.Failed;
    Executor.RemoveRunningNode(this);
    Executor.FailGraph(message, exception, this);
}
```

需要“失败分支”时，节点应使用普通输出端口：

```csharp
Complete("failure");
```

而不是调用 `Fail()`。

## 20. Cancel

外部取消：

```csharp
public void Cancel()
{
    if (Status != NodeGraphRuntimeStatus.Running
        && Status != NodeGraphRuntimeStatus.Paused)
    {
        return;
    }

    Status = NodeGraphRuntimeStatus.Cancelled;
    pendingEdges.Clear();

    foreach (var runtime in runningNodes.ToArray())
    {
        runtime.Cancel();
    }

    runningNodes.Clear();
    NotifyGraphCancelled();
}
```

NodeRuntime：

```csharp
public virtual void Cancel()
{
    if (State != NodeRuntimeState.Running)
    {
        return;
    }

    State = NodeRuntimeState.Cancelled;
    OnCancelled();
    Executor.NotifyNodeCancelled(this);
}

protected virtual void OnCancelled()
{
}
```

## 21. Stalled

只有 End 可以正常完成整个 Graph。

如果满足：

```text
pendingEdges 为空
runningNodes 为空
Status 仍然是 Running
```

说明 Graph 没有到达 End：

```csharp
private void DetectStall()
{
    if (Status == NodeGraphRuntimeStatus.Running
        && pendingEdges.Count == 0
        && runningNodes.Count == 0)
    {
        Status = NodeGraphRuntimeStatus.Stalled;
        NotifyGraphStalled();
    }
}
```

常见原因：

- 节点完成时没有触发输出端口。
- 输出端口没有连接 Edge。
- Wait 等待条件永远无法满足。
- Graph 路径没有连接到 End。

## 22. Blackboard 原则

Blackboard 与 NodeGraphAsset 完全无关。

NodeGraphAsset 不保存：

- Blackboard Key。
- Blackboard 默认值。
- Blackboard 类型定义。
- Blackboard SubAsset。
- Blackboard Editor 面板。

Blackboard 是业务系统拥有的纯运行时对象。

```text
业务系统创建 Blackboard
        ↓
写入运行数据
        ↓
创建 Executor 并传入 Blackboard
        ↓
全部 NodeRuntime 共享该 Blackboard
```

## 23. Blackboard 接口

```csharp
public interface INodeBlackboard
{
    event Action<string, object, object> ValueChanged;

    int Count { get; }

    IEnumerable<KeyValuePair<string, object>> Entries { get; }

    void Set<T>(string key, T value);

    T Get<T>(string key);

    bool TryGet<T>(string key, out T value);

    bool Contains(string key);

    bool Remove(string key);

    void Clear();
}
```

Executor 只依赖接口，业务可以提供自己的 Blackboard 实现。

## 24. 默认 Blackboard

```csharp
public sealed class NodeBlackboard : INodeBlackboard
{
    private readonly Dictionary<string, object> values = new();

    public event Action<string, object, object> ValueChanged;

    public int Count => values.Count;

    public IEnumerable<KeyValuePair<string, object>> Entries => values;

    public void Set<T>(string key, T value)
    {
        ValidateKey(key);

        values.TryGetValue(key, out var previous);
        values[key] = value;

        ValueChanged?.Invoke(key, previous, value);
    }

    public T Get<T>(string key)
    {
        if (!TryGet<T>(key, out var value))
        {
            throw new KeyNotFoundException(
                $"Blackboard key '{key}' is missing or not {typeof(T).Name}.");
        }

        return value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (values.TryGetValue(key, out var stored)
            && stored is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
}
```

`Remove()`、`Contains()`、`Clear()` 和 Key 校验按普通字典语义实现。

## 25. Blackboard 生命周期

Executor 不拥有 Blackboard 生命周期。

调用者可以为每次 Graph 执行创建独立 Blackboard：

```csharp
var blackboard = new NodeBlackboard();

blackboard.Set("Caster", caster);
blackboard.Set("Target", target);
blackboard.Set("Damage", 50f);

var executor = new NodeGraphExecutor(graph, blackboard, caster);
executor.Start();
```

也可以让多个 Executor 共享同一个 Blackboard：

```csharp
var blackboard = character.Blackboard;

var skillExecutor = new NodeGraphExecutor(skillGraph, blackboard);
var buffExecutor = new NodeGraphExecutor(buffGraph, blackboard);
```

Executor 不会在 Start、Complete、Fail 或 Cancel 时自动 Clear Blackboard。

## 26. NodeExecutionContext

```csharp
public sealed class NodeExecutionContext
{
    public NodeExecutionContext(
        NodeGraphExecutor executor,
        INodeBlackboard blackboard,
        UnityEngine.Object owner,
        object userData)
    {
        Executor = executor;
        Blackboard = blackboard;
        Owner = owner;
        UserData = userData;
    }

    public NodeGraphExecutor Executor { get; }

    public INodeBlackboard Blackboard { get; }

    public UnityEngine.Object Owner { get; }

    public object UserData { get; }
}
```

## 27. 节点读写 Blackboard

NodeDefinition 可以配置 Key：

```csharp
public sealed class DamageNodeDefinition : NodeDefinition
{
    [SerializeField]
    private string targetKey = "Target";

    [SerializeField]
    private string damageKey = "Damage";

    public string TargetKey => targetKey;

    public string DamageKey => damageKey;
}
```

NodeRuntime 使用：

```csharp
protected override void OnEnter(NodeRuntimeEdge sourceEdge)
{
    var target = Context.Blackboard.Get<GameObject>(definition.TargetKey);
    var damage = Context.Blackboard.Get<float>(definition.DamageKey);

    ApplyDamage(target, damage);
    Complete("next");
}
```

写入：

```csharp
var damage = Context.Blackboard.Get<float>(definition.DamageKey);
Context.Blackboard.Set(definition.DamageKey, damage * 1.5f);
```

Wait 的到达计数等节点内部状态不写入 Blackboard。

## 28. Runner

Executor 是普通 C# 类，不依赖 MonoBehaviour。

```csharp
public sealed class NodeGraphRunner : MonoBehaviour
{
    [SerializeField]
    private NodeGraphAsset graph;

    [SerializeField]
    private bool playOnStart;

    private NodeGraphExecutor executor;

    public NodeGraphExecutor Executor => executor;

    private void Start()
    {
        if (playOnStart)
        {
            StartGraph(new NodeBlackboard());
        }
    }

    private void Update()
    {
        executor?.Tick(Time.deltaTime);
    }

    public void StartGraph(INodeBlackboard blackboard)
    {
        executor?.Cancel();
        executor = new NodeGraphExecutor(graph, blackboard, this);
        executor.Start();
    }
}
```

业务系统可以不使用 NodeGraphRunner，直接持有和 Tick Executor。

## 29. Observer

Runtime 定义观察器接口，不引用 UnityEditor：

```csharp
public interface INodeGraphObserver
{
    void OnGraphStarted(NodeGraphExecutor executor);

    void OnNodeEntered(
        NodeGraphExecutor executor,
        NodeRuntime runtime,
        NodeRuntimeEdge sourceEdge);

    void OnNodeProgress(
        NodeGraphExecutor executor,
        NodeRuntime runtime,
        float progress);

    void OnEdgeTriggered(
        NodeGraphExecutor executor,
        NodeRuntimeEdge edge);

    void OnNodeCompleted(
        NodeGraphExecutor executor,
        NodeRuntime runtime);

    void OnNodeFailed(
        NodeGraphExecutor executor,
        NodeRuntime runtime,
        Exception exception);

    void OnNodeCancelled(
        NodeGraphExecutor executor,
        NodeRuntime runtime);

    void OnGraphStopped(
        NodeGraphExecutor executor,
        NodeGraphRuntimeStatus status);
}
```

Executor 支持添加和移除 Observer：

```csharp
public void AddObserver(INodeGraphObserver observer);

public void RemoveObserver(INodeGraphObserver observer);
```

没有 Observer 时只有一次空列表检查。

## 30. Executor 发现

为支持 Editor 查看普通 C# Executor，Runtime 提供弱引用注册事件：

```csharp
public static class NodeGraphExecutorRegistry
{
    public static event Action<NodeGraphExecutor> ExecutorCreated;

    public static event Action<NodeGraphExecutor> ExecutorDisposed;
}
```

Executor 创建后注册，Dispose 时移除。

Registry 不引用 Editor，只发送普通 C# 事件。

Editor 在 Play Mode 监听 Registry，为每个 Executor 安装调试 Observer。

## 31. Editor 调试状态

NodeGraph Editor 不枚举场景中的全部 Executor，避免大量对象导致下拉列表过长。

调试目标由 Unity 当前 Selection 决定：

```text
Project 中选中 NodeGraphAsset
    → 只显示 Graph
    → 不绑定 Executor
    → 清除运行时状态

Hierarchy 或 Inspector 中选中 NodeGraphRunner
或选中挂有 NodeGraphRunner 的 GameObject / Component
    → 自动切换到 Runner.Graph
    → 绑定 Runner.Executor
    → 显示该实例运行状态
```

NodeGraph Editor 工具栏显示当前选择：

```text
Graph    [SkillGraph]
Runner   [Player / Fireball Runner]
Status   [Running]
```

工具栏不提供全场景 Executor 下拉列表。

普通 C# Executor 仍会进入 Runtime Registry，但第一版 Editor UI 只自动绑定当前选中的 NodeGraphRunner。

NodeView 状态：

| Runtime 状态 | Editor 显示 |
|---|---|
| Idle | 默认 |
| Running | 蓝色或绿色高亮边框 |
| Completed | 短暂亮起后淡出 |
| Failed | 红色边框和错误 Tooltip |
| Cancelled | 灰色 |

ProgressNodeDefinition：

- Running 时显示进度条。
- `OnNodeProgress` 更新数值。
- 完成、失败或取消后隐藏。

## 32. Edge 调试

`OnEdgeTriggered` 时：

- 找到对应 GraphView Edge。
- 修改 Edge 颜色。
- 播放短暂高亮或流动动画。
- 保存最后触发时间。

RuntimeEdge 映射到 Editor Edge 的条件：

```text
Output Definition
Output Port ID
Input Definition
Input Port ID
```

## 33. Wait 调试

WaitAllNodeRuntime 上报：

```text
已到达入口数
总入口 Edge 数
```

NodeView 显示：

```text
Wait 2 / 3
```

等待中的入口 Edge 可以使用不同颜色显示。

## 34. Blackboard 调试

NodeGraph Editor 不在 Edit Mode 显示或编辑 Blackboard。

Play Mode 选择 Executor 后显示该实例收到的 Blackboard：

```text
Runtime Blackboard
├── Caster     Player
├── Target     Enemy_03
├── Damage     75
└── Critical   true
```

Editor 订阅：

```csharp
executor.Blackboard.ValueChanged
```

支持：

- 查看 Key。
- 查看运行时类型。
- 查看当前值。
- 值变化时短暂高亮。
- Play Mode 下修改基础类型。

Editor 修改的是当前 Blackboard 实例，不修改 NodeGraphAsset。

## 35. 调试事件历史

Editor 为每个 Executor 保存固定长度的环形事件记录。

建议默认：

```text
1024 条
```

记录内容：

```text
时间
节点
入口 Edge
出口 Edge
状态
进度
错误
Blackboard Key 变化
```

超出容量时覆盖最旧事件。

## 36. 异常处理

Executor 调用以下方法时统一捕获异常：

- `CreateRuntime()`。
- `OnEnter()`。
- `Tick()`。
- `Cancel()`。
- Observer 回调。

节点异常：

```text
NodeRuntime → Failed
Graph → Failed
清空待执行 Edge
取消其他 Running Node
通知 Editor
```

Observer 异常不能影响 Graph 执行，只记录日志。

## 37. 测试

### 37.1 Runtime Graph

- 每个 Definition 只创建一个 Runtime。
- RuntimeEdge 正确连接输入和输出。
- Start 推进所有出口 Edge。
- 同一输出 Port 的多条 Edge 全部触发。
- 普通节点完成后推进指定出口。
- 短节点链不会递归溢出。
- 立即循环触发单 Tick 调度上限。

### 37.2 End

- End 第一次 Enter 后 Graph Completed。
- End 清空待执行 Edge。
- End 取消其他 Running Runtime。
- End 不等待其他分支。

### 37.3 Wait

- Wait 等待全部 InputEdges。
- 同一 Edge 重复触发不重复计数。
- Wait 未满足时保持 Running。
- Wait 满足后推进出口。
- Wait 重新执行时清空上轮状态。

### 37.4 生命周期

- Running 节点默认禁止重入。
- Wait 允许 Running 时再次 Enter。
- Cancel 清空队列并取消 Running 节点。
- 无 Edge、无 Running 节点且未到 End 时进入 Stalled。
- 节点异常使 Graph Failed。

### 37.5 Blackboard

- Blackboard 与 Graph 无引用关系。
- Executor 使用外部传入的 Blackboard。
- Executor 不自动 Clear Blackboard。
- 两个 Executor 使用独立 Blackboard 时状态隔离。
- 两个 Executor共享 Blackboard 时可以读写同一数据。
- Get 类型不匹配时抛出明确异常。
- ValueChanged 正确提供旧值和新值。

### 37.6 Editor 调试

- 可以发现新增 Executor。
- 可以选择不同 Executor。
- Running 节点正确高亮。
- Progress 正确更新。
- Edge 触发正确高亮。
- End 进入后显示 Graph Completed。
- Wait 显示到达数量。
- Blackboard 变化正确显示。
- 切换 Executor 后清除上一个实例的显示状态。

## 38. 实现顺序

1. 实现 INodeBlackboard 和 NodeBlackboard。
2. 实现 NodeExecutionContext。
3. 实现 NodeRuntimeState 和 NodeGraphRuntimeStatus。
4. 实现 NodeRuntimeEdge。
5. 实现 NodeRuntime 基类。
6. 为 NodeDefinition 增加 CreateRuntime()。
7. 实现 Runtime Graph 两阶段构建。
8. 实现 Executor 调度队列。
9. 实现 StartNodeRuntime。
10. 实现 EndNodeRuntime。
11. 实现 Progress Node Tick。
12. 实现 WaitAllNodeDefinition 和 WaitAllNodeRuntime。
13. 实现 Complete、Fail、Cancel 和 Stalled。
14. 实现 Observer。
15. 实现 Executor Registry 和 Dispose。
16. 实现 NodeGraphRunner。
17. 添加 Runtime 单元测试。
18. Editor 增加 Executor 选择。
19. Editor 增加节点状态和 Progress 显示。
20. Editor 增加 Edge 触发显示。
21. Editor 增加 Wait 状态显示。
22. Editor 增加 Runtime Blackboard 查看。
23. 添加 Editor 调试测试。

## 39. 最终结构

```text
业务系统
├── 创建 Blackboard
├── 写入运行参数
└── 创建 Executor

NodeGraphExecutor
├── NodeRuntime Dictionary
├── NodeRuntimeEdge
├── Pending Edge Queue
├── Running Node Set
├── 外部 Blackboard
├── Context
└── Observer

NodeRuntime
├── InputEdges
├── OutputEdges
├── Enter
├── Tick
├── Complete
├── Fail
└── Cancel

Editor Debug
├── Executor 选择
├── Node 状态
├── Progress
├── Edge 触发
├── Wait 状态
├── Blackboard 当前值
└── 事件历史
```

核心原则：

> NodeRuntime 决定触发哪些出口，Executor 负责安全调度；End 首次进入立即结束全图；Wait 显式处理多分支同步；Blackboard 完全由业务层创建并注入 Executor。
