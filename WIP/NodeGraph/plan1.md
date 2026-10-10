# NodeGraph v0.1 初版设计

## 1. 目标

实现一个面向技能等顺序控制流的轻量节点框架：

- 编辑器使用 UI Toolkit + GraphView。
- Runtime 不依赖 UnityEditor、GraphView 或 Visual Scripting。
- 节点配置使用 ScriptableObject，供所有执行器共享。
- 每个执行器持有独立的运行状态和变量上下文。
- 外部程序集可以新增节点，不需要引用 Editor 程序集。
- 普通序列化字段由 Unity 自动生成编辑界面。
- 支持查看当前执行节点及长效节点进度。

## 2. v0.1 范围

### 2.1 支持

- 一个入口节点。
- 同一时间只有一个活动节点。
- 节点可以定义多个控制流输出端口。
- 每个输出端口最多连接一个目标节点。
- 短效节点和长效节点。
- 执行成功、失败和取消。
- 每个执行器独立的黑板变量。
- 外部节点自动发现。
- Play Mode 下显示当前节点和执行进度。
- 保存前进行图结构校验。

### 2.2 暂不支持

- 并行节点和 Join。
- 数据端口及数据连线。
- 子图。
- Undo/Redo。
- 复制粘贴。
- 分组、注释和 MiniMap。
- 自动布局。
- 动态增加或删除端口。
- 运行时修改图结构。
- 为每种节点单独编写字段绘制代码。

## 3. 目录与程序集

```text
Assets/FGUFW/NodeGraph/
├── Runtime/
│   ├── FGUFW.NodeGraph.Runtime.asmdef
│   ├── Data/
│   ├── Execution/
│   └── Blackboard/
├── Editor/
│   ├── FGUFW.NodeGraph.Editor.asmdef
│   ├── GraphView/
│   └── Inspectors/
└── Tests/
```

程序集依赖方向：

```text
外部节点程序集 ──> FGUFW.NodeGraph.Runtime
FGUFW.NodeGraph.Editor ──> FGUFW.NodeGraph.Runtime
```

Runtime 程序集不得引用：

- `UnityEditor`
- `UnityEditor.Experimental.GraphView`
- `Unity.VisualScripting`

## 4. 图资产

图本身是一个 ScriptableObject：

```csharp
public sealed class NodeGraphAsset : ScriptableObject
{
    [SerializeField]
    private int schemaVersion = 1;

    [SerializeField]
    private string entryNodeGuid;

    [SerializeField]
    private List<NodeDefinition> nodes = new();

    [SerializeField]
    private List<NodeEdge> edges = new();
}
```

节点作为 `NodeGraphAsset` 的 SubAsset 保存。图中只保存节点引用和连接数据，不保存执行状态。

```csharp
[Serializable]
public struct NodeEdge
{
    public string FromNodeGuid;
    public string FromPortId;
    public string ToNodeGuid;
}
```

v0.1 中每个节点只有一个隐式输入端口，因此连接不需要保存目标输入端口 ID。

## 5. 节点定义

```csharp
public abstract class NodeDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private string guid;

    [SerializeField, HideInInspector]
    private Vector2 position;

    public string Guid => guid;
    public Vector2 Position => position;

    public abstract IReadOnlyList<NodePortDefinition> OutputPorts { get; }

    public abstract NodeRuntime CreateRuntime();
}
```

`NodeDefinition` 只保存静态配置，不允许保存或修改执行状态。

```csharp
public readonly struct NodePortDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
}
```

端口 ID 是稳定标识，不应使用本地化显示名称作为 ID。

## 6. 外部自定义节点

节点菜单信息通过 Runtime 程序集中的 Attribute 声明：

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NodeMenuAttribute : Attribute
{
    public string Path { get; }
    public string DisplayName { get; }
}
```

外部节点示例：

```csharp
[NodeMenu("Flow/Wait", "等待")]
public sealed class WaitNode : NodeDefinition
{
    [SerializeField]
    private float duration;

    public override IReadOnlyList<NodePortDefinition> OutputPorts
        => NodePorts.Next;

    public override NodeRuntime CreateRuntime()
        => new WaitRuntime(duration);
}
```

外部节点只需要：

1. 继承 `NodeDefinition`。
2. 添加 `[NodeMenu]`。
3. 声明可序列化配置字段。
4. 实现 `CreateRuntime()`。

编辑器使用 `TypeCache.GetTypesDerivedFrom<NodeDefinition>()` 自动发现节点类型。TypeCache 只在 Editor 程序集中使用，不产生运行时反射开销。

## 7. 节点运行时

```csharp
public enum NodeResultType
{
    Running,
    Completed,
    Failed
}

public readonly struct NodeResult
{
    public NodeResultType Type { get; }
    public string OutputPortId { get; }

    public static NodeResult Running();
    public static NodeResult Complete(string outputPortId = null);
    public static NodeResult Fail(string outputPortId = null);
}
```

节点运行时接口：

```csharp
public abstract class NodeRuntime
{
    public virtual NodeResult Enter(NodeExecutionContext context)
        => NodeResult.Running();

    public virtual NodeResult Tick(
        NodeExecutionContext context,
        float deltaTime)
        => NodeResult.Running();

    public virtual void Exit(
        NodeExecutionContext context,
        NodeExitReason reason)
    {
    }

    public virtual float Progress => -1f;
}
```

约定：

- 短效节点通常在 `Enter()` 中返回 `Completed` 或 `Failed`。
- 长效节点在 `Enter()` 中返回 `Running`，随后由 `Tick()` 推进。
- `Progress` 返回 `0~1` 时显示进度条。
- `Progress` 返回负数时表示节点不提供进度信息。
- `Enter()` 必须重置节点上一次执行遗留的运行状态。

## 8. 执行上下文与黑板

```csharp
public sealed class NodeExecutionContext
{
    public UnityEngine.Object Owner { get; }
    public NodeBlackboard Blackboard { get; }
    public object UserData { get; }
}
```

第一版黑板使用字符串 Key，并提供运行时类型检查：

```csharp
public sealed class NodeBlackboard
{
    public void Set<T>(string key, T value);
    public T Get<T>(string key);
    public bool TryGet<T>(string key, out T value);
    public bool Remove(string key);
    public void Clear();
}
```

黑板属于执行器实例，不写入 Graph Asset，也不在多个执行器之间共享。

## 9. 执行器

核心执行器是普通 C# 类：

```csharp
public sealed class NodeGraphExecutor
{
    public GraphExecutionStatus Status { get; }
    public NodeDefinition CurrentNode { get; }
    public float CurrentProgress { get; }

    public void Start(
        NodeGraphAsset graph,
        NodeExecutionContext context);

    public void Tick(float deltaTime);

    public void Cancel();
}
```

另提供可选的 `NodeGraphRunner : MonoBehaviour`，负责在 `Update()` 中驱动执行器。业务系统也可以自行调用 `Tick()`，不强制依赖 MonoBehaviour。

### 9.1 执行流程

1. `Start()` 校验图资产并定位入口节点。
2. 为入口节点获取或创建当前执行器独占的 `NodeRuntime`。
3. 调用节点的 `Enter()`。
4. 返回 `Running` 时等待下一次 `Tick()`。
5. 返回 `Completed` 或 `Failed` 时调用 `Exit()`。
6. 根据 `OutputPortId` 查找下一节点。
7. 如果没有后续连接，则结束整个 Graph。
8. `Cancel()` 调用当前节点的 `Exit(..., Cancelled)` 并结束执行。

### 9.2 防止死循环

短效节点可以在同一帧连续跳转，但设置每帧最大跳转次数，例如 64 次。

超过限制时：

- 停止执行。
- 将 Graph 标记为 `Faulted`。
- 输出当前节点和最近跳转路径。

这用于防止纯短效节点组成的无等待循环卡死主线程。

### 9.3 Runtime 生命周期

- 每个执行器拥有独立的 NodeRuntime。
- Runtime 在节点第一次进入时延迟创建。
- 同一个执行器再次进入节点时复用 Runtime。
- `Enter()` 负责重置节点本次执行所需的状态。
- Graph 停止或销毁后释放 Runtime 引用。

## 10. GraphView 编辑器

编辑器窗口由以下部分组成：

```text
NodeGraphWindow
└── NodeGraphView : GraphView
    ├── NodeView : GraphView.Node
    └── Edge
```

GraphView 类型只存在于 Editor 程序集，不暴露给外部节点 API。

### 10.1 创建节点

1. 使用 TypeCache 获取可创建节点类型。
2. 根据 `NodeMenuAttribute.Path` 构建搜索菜单。
3. 使用 `ScriptableObject.CreateInstance(nodeType)` 创建节点。
4. 生成稳定 GUID。
5. 使用 `AssetDatabase.AddObjectToAsset()` 添加到 Graph Asset。
6. 将节点引用加入 `NodeGraphAsset.nodes`。
7. 创建对应 NodeView。

### 10.2 自动绘制节点字段

NodeView 使用 Unity 默认序列化 UI：

```csharp
var serializedNode = new SerializedObject(nodeDefinition);
var inspector = new InspectorElement(serializedNode);

extensionContainer.Add(inspector);
RefreshExpandedState();
```

普通外部节点不需要编写任何 Editor 代码。

以下字段不会显示在节点面板中：

- GUID。
- 编辑器位置。
- 内部版本字段。

复杂节点如果确实需要特殊 UI，可以在独立的可选 Editor 程序集中提供 PropertyDrawer，但这不是 v0.1 的必要能力。

### 10.3 保存策略

- 节点移动时更新 `NodeDefinition.position`。
- 连线变化时更新 `NodeGraphAsset.edges`。
- 字段编辑通过 `SerializedObject` 写回节点 SubAsset。
- 修改后调用 `EditorUtility.SetDirty()`。
- 不主动接入 Undo/Redo。
- 删除节点时弹出确认，避免不可恢复的误删。
- 每次打开窗口时，根据 Graph Asset 完整重建 GraphView。

## 11. 运行态调试

执行器通过普通 C# 事件发布状态，不引用 UnityEditor：

```csharp
public event Action<NodeExecutionSnapshot> StateChanged;
```

```csharp
public readonly struct NodeExecutionSnapshot
{
    public GraphExecutionStatus GraphStatus { get; }
    public string NodeGuid { get; }
    public NodeResultType NodeStatus { get; }
    public float Progress { get; }
}
```

Play Mode 下，编辑器工具栏允许选择一个 `NodeGraphRunner`：

- 根据 Node GUID 高亮当前节点。
- 显示 Running、Completed、Failed 或 Cancelled。
- `Progress` 位于 `0~1` 时在节点上显示进度条。
- 多个执行器运行同一个 Graph 时，只显示当前选中执行器的状态。

## 12. 图校验

保存或运行前至少检查：

- 节点引用不为空。
- 节点 GUID 不为空且唯一。
- 入口节点存在。
- Edge 的起点和终点存在。
- Edge 引用的输出端口存在。
- 每个输出端口最多存在一条连接。
- 没有重复 Edge。
- 节点 SubAsset 属于当前 Graph Asset。
- 节点脚本没有丢失。

循环本身允许存在，由单帧最大跳转次数保护主线程。

## 13. 错误处理

- 节点 `Enter()`、`Tick()` 或 `Exit()` 抛出异常时，由执行器捕获。
- 执行器状态切换为 `Faulted`。
- 记录 Graph、节点 GUID、节点类型和异常。
- 不继续执行后续节点。
- `Exit()` 异常不能覆盖之前已经记录的原始异常。

## 14. 初始节点

第一阶段只实现用于验证框架的节点：

- `LogNode`：短效节点，输出日志。
- `WaitNode`：长效节点，提供进度。
- `BranchNode`：根据黑板布尔值选择输出端口。
- `SetBlackboardNode`：设置测试变量。
- `FailureNode`：主动返回失败。

这些节点用于验证：

- 节点自动发现。
- SubAsset 创建与保存。
- 默认字段 UI。
- 短效节点连续跳转。
- 长效节点 Tick 和进度。
- 分支连接。
- 黑板隔离。
- 取消执行。
- 运行态节点高亮。

## 15. 测试

### 15.1 Runtime 测试

- 线性短效节点执行顺序。
- 长效节点跨帧执行。
- 成功和失败输出分支。
- Cancel 调用当前节点 Exit。
- 两个执行器共享同一 Graph 时状态互不影响。
- 同一节点重新进入时状态正确重置。
- 无连接输出正确结束 Graph。
- 短效循环触发单帧跳转上限。
- 节点异常使 Graph 进入 Faulted。

### 15.2 Editor 测试

- 外部程序集节点可以被 TypeCache 发现。
- 节点可以创建为 Graph SubAsset。
- 节点字段可以通过 InspectorElement 自动编辑。
- 保存并重新打开后节点位置和连接不丢失。
- 删除节点时相关 Edge 一并删除。
- 非法 Graph 能显示明确的校验错误。

## 16. 实现顺序

1. 创建 Runtime 和 Editor asmdef。
2. 实现 Graph Asset、NodeDefinition、NodeEdge。
3. 实现 NodeRuntime、NodeResult 和 NodeGraphExecutor。
4. 实现黑板和执行上下文。
5. 为 Runtime 添加单元测试。
6. 实现 GraphView 窗口、NodeView 和连线保存。
7. 实现 TypeCache 节点菜单和 SubAsset 创建。
8. 接入 InspectorElement 自动字段绘制。
9. 实现测试节点。
10. 实现 Play Mode 调试高亮和进度显示。

完成上述内容后，再根据实际技能系统需求决定是否增加并行、子图或更严格的黑板变量定义。
