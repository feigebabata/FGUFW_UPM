# NodeGraph Plan 2：Editor First

## 1. 目标

第一阶段只完成一个可用的 GraphView 节点编辑器。

- 编辑器使用 UI Toolkit + GraphView。
- 节点支持多个输入端口和多个输出端口。
- 所有端口都允许连接多条 Edge。
- 支持 GraphView 自带的 Copy、Cut、Paste、Delete 和 Duplicate 菜单。
- 节点使用 ScriptableObject 保存为 Graph Asset 的 SubAsset。
- 外部程序集可以定义节点，不需要引用 Editor 程序集。
- 节点序列化字段由 Unity 自动生成编辑界面。
- 需要显示进度的节点通过 `ProgressNodeDefinition` 标记，NodeView 自动创建进度条。
- Graph 使用内置 Start 和 End 节点明确表示入口与出口。
- Start 节点使用绿色标题，End 节点使用红色标题，普通节点保持默认配色。
- Runtime 只保留编辑器所需的数据结构，不实现节点执行。

本方案不继承 `plan1` 的执行器、黑板和运行状态设计。第一阶段只创建进度节点的进度条 UI，不提供真实运行进度数据源。

## 2. 第一阶段范围

### 2.1 支持

- 创建和打开 Graph Asset。
- 网格背景、平移和缩放。
- Snap 按钮：居中全部节点，大图自动缩放到可见范围。
- 初次打开、EditorWindow 重建、Editor 启动恢复和 Graph/Runner 选择切换时自动 Snap。
- 单选和框选。
- 创建、移动和删除节点。
- 多输入、多输出端口。
- 所有端口允许多链接。
- 创建和删除连线。
- 从 Output Port 拖线到空白处时打开节点创建窗口。
- 创建节点后自动连接第一个类型兼容的 Input Port。
- 端口类型检查。
- 防止完全重复的连线。
- Copy。
- Cut。
- Paste。
- Duplicate。
- Delete。
- 跨 Graph 复制粘贴。
- 节点字段自动绘制。
- 进度节点的进度条 UI。
- 自动创建 Start 和 End 节点。
- Start 和 End 专用节点样式。
- 保存、重新打开和结构校验。

### 2.2 暂不支持

- 节点运行时。
- Node Executor。
- Blackboard。
- Context。
- Runner。
- 真实节点运行状态和进度数据源。
- Undo/Redo。
- 动态端口。
- 数据类型自动转换。
- 子图。
- Group。
- Sticky Note。
- MiniMap。
- 自动布局。
- 运行时编辑 Graph。

## 3. 目录和程序集

```text
Assets/FGUFW/NodeGraph/
├── Runtime/
│   ├── FGUFW.NodeGraph.Runtime.asmdef
│   ├── Graph/
│   │   ├── NodeGraphAsset.cs
│   │   ├── NodeEdge.cs
│   │   └── NodeGraphValidation.cs
│   └── Node/
│       ├── NodeDefinition.cs
│       ├── ProgressNodeDefinition.cs
│       ├── NodePortDefinition.cs
│       ├── NodeMenuAttribute.cs
│       ├── FlowPort.cs
│       └── BuiltIn/
│           ├── StartNodeDefinition.cs
│           ├── EndNodeDefinition.cs
│           ├── WaitAllNodeDefinition.cs
│           ├── DelayNodeDefinition.cs
│           └── LogNodeDefinition.cs
├── Editor/
│   ├── FGUFW.NodeGraph.Editor.asmdef
│   ├── Graph/
│   │   ├── NodeGraphEditorWindow.cs
│   │   ├── NodeGraphView.cs
│   │   ├── NodeClipboard.cs
│   │   └── NodeGraph.uss
│   └── Node/
│       ├── NodeView.cs
│       ├── NodeSearchProvider.cs
│       └── NodeTypeRegistry.cs
└── Tests/
```

依赖关系：

```text
外部节点程序集 ──> FGUFW.NodeGraph.Runtime
FGUFW.NodeGraph.Editor ──> FGUFW.NodeGraph.Runtime
```

Runtime 不得引用：

- `UnityEditor`
- `UnityEditor.Experimental.GraphView`
- `Unity.VisualScripting`

## 4. Graph 数据结构

```csharp
public sealed class NodeGraphAsset : ScriptableObject
{
    [SerializeField]
    private int schemaVersion = 1;

    [SerializeField]
    private List<NodeDefinition> nodes = new();

    [SerializeField]
    private List<NodeEdge> edges = new();

    public int SchemaVersion => schemaVersion;
    public IReadOnlyList<NodeDefinition> Nodes => nodes;
    public IReadOnlyList<NodeEdge> Edges => edges;
}
```

不使用 `[CreateAssetMenu]` 直接创建，因为创建 Graph 时还需要同步创建 Start 和 End SubAsset。

Editor 提供自定义菜单：

```text
Assets/Create/FGUFW/Node Graph
```

创建流程：

1. 创建 NodeGraphAsset。
2. 创建唯一的 StartNodeDefinition SubAsset。
3. 创建一个 EndNodeDefinition SubAsset。
4. 设置默认位置。
5. 加入节点列表。
6. 保存资产。

默认不自动连接 Start 和 End。

Graph Asset 保存：

- 节点 SubAsset 引用。
- 节点之间的 Edge。
- 数据格式版本。

Graph Asset 不保存：

- GraphView 的 Node、Port 和 Edge 对象。
- 选中状态。
- 运行状态。
- 运行时上下文。

每次打开编辑器时，都根据 Graph Asset 完整重建 GraphView。

## 5. 节点定义

```csharp
public abstract class NodeDefinition : ScriptableObject
{
    [SerializeField, HideInInspector]
    private Vector2 position;

    public Vector2 Position => position;

    public abstract IReadOnlyList<NodePortDefinition> Ports { get; }
}
```

`NodeDefinition` 只保存节点配置和编辑器位置。

节点不包含：

- Execute。
- Tick。
- Enter。
- Exit。
- Runtime State。

### 5.1 进度节点定义

需要显示进度的节点使用独立的标记基类：

```csharp
public abstract class ProgressNodeDefinition : NodeDefinition
{
}
```

`ProgressNodeDefinition` 不增加运行状态字段，也不保存进度值。

它只表达：

```text
该节点需要显示执行进度
NodeView 需要提供进度显示区域
```

Editor 创建 NodeView 时进行判断：

```csharp
if (definition is ProgressNodeDefinition)
{
    CreateProgressBar();
}
```

普通 `NodeDefinition` 不创建进度条。

进度值属于具体执行实例，不能写入共享的 ScriptableObject 节点配置。第一阶段只在 NodeView 提供临时显示接口：

```csharp
public void SetProgress(float progress)
{
    progressBar.value = Mathf.Clamp01(progress);
    progressBar.title =
        $"{Mathf.RoundToInt(progressBar.value * 100f)}%";
    progressBar.style.display = DisplayStyle.Flex;
}

public void ClearProgress()
{
    progressBar.style.display = DisplayStyle.None;
}
```

以后实现运行时调试时，由 Editor 调试桥接层根据 `NodeDefinition` 对象引用找到对应 NodeView，再调用 `SetProgress()`。

第一阶段不定义进度如何计算，也不定义运行时如何发送进度。

### 5.2 Start 节点

Start 是框架内置的唯一入口节点：

```csharp
[NodeMenu("Graph/Start", "Start")]
public sealed class StartNodeDefinition : NodeDefinition
{
    private static readonly NodePortDefinition[] PortDefinitions =
    {
        new(
            "start",
            "Start",
            NodePortDirection.Output,
            typeof(FlowPort))
    };

    public override IReadOnlyList<NodePortDefinition> Ports =>
        PortDefinitions;
}
```

规则：

- 一个 Graph 必须且只能存在一个 Start。
- Start 没有输入端口。
- Start 有一个 Flow 输出端口。
- 由于所有 Port 都是 Multi，Start 可以连接多个后续节点。
- Start 不显示普通字段区域。
- Start 不能 Copy、Cut、Duplicate 或 Delete。
- Start 可以被选中和移动。
- Start 不出现在普通创建节点菜单中。
- 新建 Graph Asset 时由 Editor 自动创建。

Start NodeView 的标题区域添加：

```text
node-title--start
```

USS 使用绿色标题：

```css
.node-title--start {
    background-color: rgb(18, 122, 53);
    border-color: rgb(8, 72, 29);
    color: rgb(235, 255, 240);
}
```

### 5.3 End 节点

End 是框架内置的分支结束节点：

```csharp
[NodeMenu("Graph/End", "End")]
public sealed class EndNodeDefinition : NodeDefinition
{
    private static readonly NodePortDefinition[] PortDefinitions =
    {
        new(
            "end",
            "End",
            NodePortDirection.Input,
            typeof(FlowPort))
    };

    public override IReadOnlyList<NodePortDefinition> Ports =>
        PortDefinitions;
}
```

规则：

- 一个 Graph 必须且只能存在一个 End。
- End 有一个 Flow 输入端口。
- End 没有输出端口。
- End 的输入端口允许多条连接。
- End 不显示普通字段区域。
- End 不能 Copy、Cut、Duplicate 或 Delete。
- End 可以被选中和移动。
- End 不出现在普通创建节点菜单中。
- 新建 Graph Asset 时由 Editor 自动创建一个默认 End。

End NodeView 的标题区域添加：

```text
node-title--end
```

USS 使用红色标题：

```css
.node-title--end {
    background-color: rgb(179, 38, 46);
    border-color: rgb(102, 18, 24);
    color: rgb(255, 238, 238);
}
```

普通节点不添加 Start/End 样式 Class，继续使用 GraphView 默认节点配色。

### 5.4 入口和结束语义

`NodeGraphAsset` 不额外保存 Start 引用，而是从节点列表识别：

```csharp
public StartNodeDefinition GetStartNode()
{
    return nodes.OfType<StartNodeDefinition>().SingleOrDefault();
}

public EndNodeDefinition GetEndNode()
{
    return nodes.OfType<EndNodeDefinition>().SingleOrDefault();
}
```

Editor 阶段只负责：

- 保证一个 Graph 只有一个 Start。
- 保证一个 Graph 只有一个 End。
- Start 明确 Graph 入口。
- End 明确单个执行分支的出口。

未来实现运行时后：

- 从 Start 的输出 Edge 开始执行。
- 到达 End 表示当前分支结束。
- 所有活动执行分支都结束后，整个 Graph 才结束。
- 多个分支汇合后的等待行为由未来专用 Join 节点定义，End 不承担 Join 语义。

## 6. 端口定义

Runtime 程序集定义自己的方向枚举，不能引用 GraphView 的 `Direction`。

```csharp
public enum NodePortDirection
{
    Input,
    Output
}
```

```csharp
public readonly struct NodePortDefinition
{
    public NodePortDefinition(
        string id,
        string displayName,
        NodePortDirection direction,
        Type valueType)
    {
        Id = id;
        DisplayName = displayName;
        Direction = direction;
        ValueType = valueType;
    }

    public string Id { get; }
    public string DisplayName { get; }
    public NodePortDirection Direction { get; }
    public Type ValueType { get; }
}
```

约束：

- 端口 ID 不能为空。
- 同一节点内端口 ID 必须唯一。
- 端口 ID 是稳定标识，不能使用本地化名称。
- `DisplayName` 只用于显示。
- `ValueType` 用于编辑器连接兼容检查。
- 第一版端口列表由节点类型固定声明，不支持字段变化后动态增减端口。

控制流端口使用一个空标记类型：

```csharp
public sealed class FlowPort
{
    private FlowPort()
    {
    }
}
```

## 7. 外部节点

菜单信息通过 Runtime Attribute 声明：

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NodeMenuAttribute : Attribute
{
    public NodeMenuAttribute(string path, string displayName = null)
    {
        Path = path;
        DisplayName = displayName;
    }

    public string Path { get; }
    public string DisplayName { get; }
}
```

外部节点示例：

```csharp
[NodeMenu("Flow/Branch", "Branch")]
public sealed class BranchNode : NodeDefinition
{
    private static readonly NodePortDefinition[] PortDefinitions =
    {
        new(
            "enter",
            "Enter",
            NodePortDirection.Input,
            typeof(FlowPort)),

        new(
            "condition",
            "Condition",
            NodePortDirection.Input,
            typeof(bool)),

        new(
            "true",
            "True",
            NodePortDirection.Output,
            typeof(FlowPort)),

        new(
            "false",
            "False",
            NodePortDirection.Output,
            typeof(FlowPort))
    };

    [SerializeField]
    private string description;

    public override IReadOnlyList<NodePortDefinition> Ports =>
        PortDefinitions;
}
```

外部节点只需要：

1. 继承 `NodeDefinition`。
2. 添加 `[NodeMenu]`。
3. 声明端口。
4. 声明可序列化配置字段。

不需要 Editor 程序集。

外部进度节点示例：

```csharp
[NodeMenu("Flow/Wait", "Wait")]
public sealed class WaitNode : ProgressNodeDefinition
{
    private static readonly NodePortDefinition[] PortDefinitions =
    {
        new(
            "enter",
            "Enter",
            NodePortDirection.Input,
            typeof(FlowPort)),

        new(
            "complete",
            "Complete",
            NodePortDirection.Output,
            typeof(FlowPort))
    };

    [SerializeField]
    private float duration = 1f;

    public override IReadOnlyList<NodePortDefinition> Ports =>
        PortDefinitions;
}
```

`WaitNode` 不需要 Editor 代码，NodeView 会根据其继承关系自动添加进度条。

## 8. Edge 数据

Node 是 ScriptableObject SubAsset，因此 Edge 直接保存节点对象引用，同时保存起点和终点端口 ID：

```csharp
[Serializable]
public struct NodeEdge
{
    public NodeDefinition OutputNode;
    public string OutputPortId;

    public NodeDefinition InputNode;
    public string InputPortId;
}
```

不额外维护节点 GUID。

Unity 负责序列化 SubAsset 之间的对象引用。Editor 使用对象引用作为节点身份：

```csharp
Dictionary<NodeDefinition, NodeView> nodeViews;
```

判断两条 Edge 是否相同时，比较：

- `OutputNode` 对象引用。
- `OutputPortId`。
- `InputNode` 对象引用。
- `InputPortId`。

一个端口可以出现在任意数量的 Edge 中：

```text
A.Output → B.Input
A.Output → C.Input
A.Output → D.Input

X.Output → B.Input
Y.Output → B.Input
```

完全相同的 Edge 只能存在一条：

```text
A.Output → B.Input
```

不能重复创建第二条。

## 9. GraphView 端口创建

NodeView 根据 `NodeDefinition.Ports` 创建 GraphView Port。

```csharp
var direction = definition.Direction == NodePortDirection.Input
    ? Direction.Input
    : Direction.Output;

var port = InstantiatePort(
    Orientation.Horizontal,
    direction,
    Port.Capacity.Multi,
    definition.ValueType);
```

所有 Port 统一使用：

```csharp
Port.Capacity.Multi
```

不提供 Single/Multiple 配置。

输入端口放入：

```csharp
inputContainer
```

输出端口放入：

```csharp
outputContainer
```

每个 GraphView Port 的 `userData` 保存对应的端口 ID。

## 10. 端口兼容规则

`GetCompatiblePorts()` 检查：

1. 不能连接端口自身。
2. 不能连接同一个节点上的端口。
3. Input 只能连接 Output。
4. `portType` 必须完全相同。
5. 目标 Edge 不能已经存在。

第一版使用严格类型相等：

```csharp
startPort.portType == targetPort.portType
```

暂不支持：

- 派生类到基类。
- 接口兼容。
- `int` 到 `float`。
- 自动转换节点。

Graph 允许不同节点之间形成循环。

### 10.1 从输出端口创建节点

Output Port 的 `EdgeConnector` 使用自定义 `IEdgeConnectorListener`。

当 Edge 拖到空白区域时：

1. 读取正在拖拽的 Output Port。
2. 记录 Port 类型和落点。
3. 打开节点 SearchWindow。
4. 只显示至少包含一个同类型 Input Port 的节点。
5. 创建节点。
6. 按节点声明顺序查找第一个兼容 Input Port。
7. 自动创建并保存 Edge。

从 Input Port 拖到空白区域时不打开节点创建窗口。

## 11. GraphView 自带菜单

GraphView 提供菜单和快捷键外壳，但复制数据逻辑需要编辑器实现。

NodeGraphView 注册：

```csharp
serializeGraphElements = SerializeElements;
canPasteSerializedData = CanPaste;
unserializeAndPaste = UnserializeAndPaste;
deleteSelection = DeleteSelection;
```

对应菜单：

| 菜单 | 行为 |
|---|---|
| Copy | 序列化选中的节点和内部 Edge |
| Cut | Copy 后删除选中元素 |
| Paste | 从剪贴板创建新节点和内部 Edge |
| Duplicate | 使用同一套序列化逻辑复制并偏移节点 |
| Delete | 删除选中节点或 Edge |

没有有效选择时，相关菜单保持禁用。

## 12. 复制范围规则

复制时以选中的 NodeView 为准。

- 复制所有选中的节点。
- Edge 不要求被单独选中。
- 当 Edge 两端节点都被选中时，自动复制该 Edge。
- 只有一端节点被选中时，默认不复制该 Edge。
- 当边界 Edge 本身也被明确选中时，记录其外部端点。
- 只选择 Edge 而没有选择节点时，Copy 不产生有效剪贴板数据。
- Start 节点不进入剪贴板数据。
- End 节点不进入剪贴板数据。
- Start 或 End 与其他选中节点之间的 Edge 默认视为边界 Edge；明确选中该 Edge 时可以复制连接关系。

示例：

```text
外部 X → A → B → 外部 Y
          └→ C
```

选中 `A、B、C` 后复制：

```text
A' → B'
└──→ C'
```

不会复制：

```text
X → A'
B' → Y
```

未明确选中的边界 Edge 不复制，避免普通 Paste 隐式修改现有节点的连接关系。

明确选中的边界 Edge：

- 粘贴回同一个 Graph 时，重新连接到原外部节点。
- 粘贴到另一个 Graph 时，外部节点不属于目标 Graph，因此跳过该边界 Edge。

## 13. 剪贴板格式

```csharp
[Serializable]
public sealed class GraphClipboardData
{
    public int Version;
    public List<ClipboardNode> Nodes = new();
    public List<ClipboardEdge> Edges = new();
}
```

```csharp
[Serializable]
public sealed class ClipboardNode
{
    public int LocalId;
    public string TypeName;
    public string Json;
    public Vector2 RelativePosition;
}
```

```csharp
[Serializable]
public sealed class ClipboardEdge
{
    public int OutputNodeId;
    public string OutputExternalId;
    public string OutputPortId;
    public int InputNodeId;
    public string InputExternalId;
    public string InputPortId;
}
```

`LocalId` 只在当前剪贴板数据中有效，不写入 Graph Asset。

复制时为选中的节点临时分配连续编号：

```text
Node A → LocalId 0
Node B → LocalId 1
Node C → LocalId 2
```

ClipboardEdge 优先使用临时编号表达新节点之间的连接。明确选中的边界 Edge 使用 `GlobalObjectId` 临时记录外部节点，只用于 Editor 剪贴板，不写入 Graph Asset。

剪贴板字符串添加固定标记：

```text
FGUFW_NODE_GRAPH_V1
{ JSON }
```

`CanPasteSerializedData()` 只检查带有正确标记、版本和合法 JSON 的数据。

## 14. Copy

Copy 流程：

1. 从 GraphElement 集合中筛选 NodeView。
2. 如果没有 NodeView，返回空字符串。
3. 计算所选节点位置包围盒或中心点。
4. 为每个节点分配临时 `LocalId`。
5. 使用 `EditorJsonUtility.ToJson()` 序列化节点字段。
6. 保存节点类型的 Assembly Qualified Name。
7. 保存节点相对位置。
8. 使用 LocalId 保存两端都在选择范围内的 Edge。
9. 生成带固定标记的 JSON。

复制数据不保存：

- 原 Graph Asset。
- GraphView 对象。
- 选中状态。
- 运行状态。

## 15. Paste

Paste 采用全有或全无策略。

### 15.1 预检查

1. 检查剪贴板标记。
2. 检查数据版本。
3. 解析 JSON。
4. 检查所有节点类型都存在。
5. 检查类型都继承 `NodeDefinition`。
6. 检查端口定义合法。

任何节点类型缺失时，整个 Paste 失败，不创建半成品。

### 15.2 创建节点

1. 创建全部 NodeDefinition 实例。
2. 使用 `EditorJsonUtility.FromJsonOverwrite()` 恢复配置字段。
3. 根据粘贴锚点恢复新位置。
4. 添加为目标 Graph Asset 的 SubAsset。
5. 建立 `LocalId` 到新 NodeDefinition 对象的映射。

```csharp
Dictionary<int, NodeDefinition> pastedNodes;
```

节点必须全部创建完成后，才能创建 Edge。

### 15.3 创建 Edge

1. 使用 LocalId 映射找到新节点对象。
2. 检查输出和输入端口仍然存在。
3. 检查端口方向。
4. 检查端口类型。
5. 检查完全相同的 Edge 是否已经存在。
6. 创建新的 NodeEdge。

### 15.4 完成

1. 标记 Graph 和节点为 Dirty。
2. 重建或增量添加 NodeView 和 Edge。
3. 清空旧选择。
4. 选中所有新节点。

## 16. Paste 位置

NodeGraphView 记录最后一次鼠标在 Graph 坐标系中的位置。

普通 Paste：

- 以最后鼠标位置作为粘贴中心。
- 鼠标不在 GraphView 内时，使用当前视口中心。

Duplicate：

- 在原节点位置基础上偏移。

默认偏移：

```text
(30, 30)
```

## 17. Cut

Cut 使用同一份 Copy 数据，然后调用 Delete。

删除时：

- 删除选中节点。
- 删除所有与这些节点关联的 Edge。
- 删除选中的独立 Edge。
- 从 Graph Asset 的节点列表中移除节点。
- 删除节点 SubAsset。
- 跳过受保护的 Start 和 End。

Cut 产生的剪贴板只保留选择范围内部 Edge。

## 18. Delete

删除 NodeView：

1. 找到对应 NodeDefinition。
2. 如果是 Start 或 End，拒绝删除。
3. 删除所有输入和输出 Edge。
4. 从 Graph Asset 移除节点引用。
5. 删除节点 SubAsset。
6. 从 GraphView 移除 NodeView。

删除 Edge：

1. 根据四个端点标识找到 NodeEdge。
2. 从 Graph Asset 删除。
3. 从 GraphView 删除。

第一版不接入 Undo。

## 19. Duplicate

Duplicate 使用 Copy/Paste 相同的数据格式。

区别：

- 目标 Graph 必须是当前 Graph。
- 节点位置偏移 `(30, 30)`。
- 复制后选中新节点。
- 只恢复选择范围内部 Edge。
- Start 和 End 不参与 Duplicate。

每次 Duplicate 都创建全新的 NodeDefinition SubAsset，不能复用原节点对象引用。

## 20. 节点字段自动绘制

NodeView 使用：

```csharp
new InspectorElement(new SerializedObject(nodeDefinition))
```

Unity 自动处理：

- `public` 字段。
- `[SerializeField]` 私有字段。
- Enum。
- `UnityEngine.Object` 引用。
- 数组。
- `List<T>`。
- 可序列化嵌套类型。
- PropertyDrawer。

隐藏字段：

- Position。
- 内部版本字段。

复杂节点以后可以在独立 Editor 程序集中提供 PropertyDrawer，但不属于第一阶段。

### 20.1 进度节点进度条

当节点继承 `ProgressNodeDefinition` 时，NodeView 在字段区域之后创建一个通用 `ProgressBar`。

进度条规则：

- 普通节点不创建进度条。
- 进度节点创建进度条。
- 没有调试数据时默认隐藏。
- `SetProgress()` 后显示。
- 进度值限制在 `0~1`。
- Graph 切换、退出 Play Mode 或调试对象丢失时调用 `ClearProgress()`。
- 进度值不写入 NodeDefinition。
- Copy/Paste 不复制进度值。

第一阶段可以通过 Editor 测试直接调用 `SetProgress()` 验证绘制，不需要实现运行时。

## 21. 节点创建

编辑器使用：

```csharp
TypeCache.GetTypesDerivedFrom<NodeDefinition>()
```

筛选：

- 非抽象类。
- 非泛型定义。
- 可以创建 ScriptableObject。
- 排除 `StartNodeDefinition` 和 `EndNodeDefinition`，因为它们只能由 Graph 创建流程生成。

创建流程：

1. 读取 `NodeMenuAttribute`。
2. 构建 SearchWindow 菜单。
3. `ScriptableObject.CreateInstance(type)`。
4. 设置节点位置。
5. `AssetDatabase.AddObjectToAsset()`。
6. 加入 Graph Asset。
7. 创建 NodeView。
8. 标记资产为 Dirty。

创建 NodeView 时：

```csharp
if (definition is StartNodeDefinition)
{
    titleContainer.AddToClassList("node-title--start");
    capabilities &= ~Capabilities.Copiable;
    capabilities &= ~Capabilities.Deletable;
}
else if (definition is EndNodeDefinition)
{
    titleContainer.AddToClassList("node-title--end");
    capabilities &= ~Capabilities.Copiable;
    capabilities &= ~Capabilities.Deletable;
}
```

Start 和 End 没有普通配置字段，因此不创建 InspectorElement 字段区域，只显示标题和端口。

## 22. 保存和加载

保存内容：

- Graph schema version。
- Node SubAsset。
- Node Position。
- Node 配置字段。
- Edge 的两个 NodeDefinition 引用和两个 Port ID。

不保存 GraphView 实例。

加载流程：

1. 清空当前 GraphView。
2. 为每个 NodeDefinition 创建 NodeView。
3. 根据端口定义创建 Port。
4. 建立 `(NodeDefinition, PortId)` 到 Port 的索引。
5. 根据 NodeEdge 创建 GraphView Edge。

## 23. 校验

至少检查：

- Node 引用不为空。
- Graph 节点列表中没有重复的 NodeDefinition 引用。
- 节点确实属于当前 Graph Asset。
- 恰好存在一个 StartNodeDefinition。
- 恰好存在一个 EndNodeDefinition。
- Start 没有 Input Port。
- Start 至少有一个 Flow Output Port。
- End 至少有一个 Flow Input Port。
- End 没有 Output Port。
- 端口 ID 不为空。
- 同一节点端口 ID 唯一。
- Edge 起点节点存在。
- Edge 终点节点存在。
- Edge 的起点和终点节点都存在于当前 Graph 的节点列表中。
- 输出端口存在。
- 输入端口存在。
- 端口方向正确。
- 端口类型一致。
- 没有完全重复的 Edge。
- 没有缺失脚本的节点。

Graph 循环允许存在。

## 24. Editor 样式

第一阶段样式只保证清晰可用：

- 可见网格。
- 深色背景。
- 输入端口统一位于左侧。
- 输出端口统一位于右侧。
- 节点标题清晰。
- 节点字段区域可折叠。
- 进度节点具有统一进度条区域。
- Start 节点标题使用绿色。
- End 节点标题使用红色。
- 普通节点不附加专用颜色，保持 GraphView 默认配色。
- 选中节点有明确边框。

不以复刻 Shader Graph 外观为第一阶段目标。

## 25. 测试

### 25.1 数据测试

- 创建 Graph Asset。
- 添加和删除 Node SubAsset。
- Graph 节点列表不允许重复对象引用。
- Edge 正确保存两个节点引用和两个端口 ID。
- 新建 Graph 自动包含一个 Start 和一个 End。
- Graph 不允许出现第二个 Start。
- Graph 不允许出现第二个 End。
- 重复 Edge 被拒绝。
- 非法端口连接被拒绝。

### 25.2 Editor 测试

- 外部节点能被 TypeCache 发现。
- 多输入、多输出 Port 正确生成。
- 同一 Port 可以连接多条 Edge。
- 不同类型端口不能连接。
- 普通节点不创建进度条。
- 进度节点自动创建进度条。
- 进度节点的进度条可以设置和清除。
- Start 使用绿色标题且没有字段区域。
- End 使用红色标题且没有字段区域。
- 普通节点保持默认配色。
- Start 不能删除、Cut、Copy 或 Duplicate。
- End 不能删除、Cut、Copy 或 Duplicate。
- Graph 保存并重新加载后结构不丢失。
- 删除节点会删除全部相关 Edge。

### 25.3 Clipboard 测试

- 单节点 Copy/Paste。
- 多节点 Copy/Paste。
- 内部 Edge 被保留。
- 未选择的边界 Edge 不被复制。
- 明确选中的边界 Edge 在同一 Graph 粘贴时被恢复。
- 明确选中的边界 Edge 跨 Graph 粘贴时被跳过。
- Start 不进入剪贴板。
- End 不进入剪贴板。
- Duplicate 创建新的 NodeDefinition SubAsset。
- Duplicate 不复用原节点对象引用。
- Duplicate 位置正确偏移。
- Cut 后原节点和相关 Edge 被删除。
- 可以粘贴到另一个 Graph。
- 缺失节点类型时拒绝整个 Paste。
- 非 NodeGraph 剪贴板数据不能粘贴。

## 26. 实现顺序

1. 创建 Runtime 和 Editor asmdef。
2. 实现基础数据结构和 Start/End 节点。
3. 实现自定义 Graph Asset 创建流程，自动创建 Start 和 End。
4. 实现 NodeGraphAsset SubAsset 管理。
5. 实现 NodeGraphEditorWindow。
6. 实现 NodeGraphView、网格、缩放和选择。
7. 实现外部节点发现和 SearchWindow。
8. 实现 NodeView、多输入、多输出 Port 和进度节点进度条。
9. 实现 Start/End 专用样式和操作限制。
10. 实现 Edge 创建、删除和校验。
11. 实现 Graph 保存和重建。
12. 实现 `serializeGraphElements`。
13. 实现 `canPasteSerializedData`。
14. 实现 `unserializeAndPaste`。
15. 实现 `deleteSelection`。
16. 验证 GraphView 自带 Copy、Cut、Paste、Delete 和 Duplicate。
17. 添加数据、Editor 和 Clipboard 测试。

完成第一阶段后，再根据实际节点业务讨论 Runtime 执行模型。
