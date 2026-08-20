# FGUFW Local Save

轻量本地存档服务，将多个不同类型的存档对象保存到同一个文件。

## Packages\manifest.json
```
"com.fgufw.local-save": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.local-save#local-save@1.0.0",
"com.fgufw.core": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.core#core@1.0.2"
```

## 特性

- 实现 `ISaveService`
- 每种类型保存一个对象实例
- 不存在的数据通过 `new T()` 创建
- 延迟反序列化，只在首次 `Get<T>()` 时恢复具体类型
- 未加载的旧条目会在再次保存时保留
- 应用正常退出时同步保存
- 托管未处理异常时尝试同步保存
- 应用进入后台时同步保存
- 默认保存路径：`Application.persistentDataPath/FGUFW/LocalSave.json`

## 依赖

- `com.fgufw.core`
- 任意已注册的 `IJsonService` 实现，例如 `com.fgufw.litjson`

## 使用

```csharp
await fg.save.LoadAsync();

var saveData = fg.save.Get<GameSaveData>();
saveData.Coin += 10;

await fg.save.SaveAsync();
```

存档类型必须是具有公开无参构造函数的普通引用类型。

```csharp
[Serializable]
public sealed class GameSaveData
{
    public int Coin;
}
```

进程被强制结束、设备断电或发生原生崩溃时，应用代码没有执行机会，无法保证退出瞬间保存。重要数据仍应在业务发生变化后主动调用 `SaveAsync()`。

## 禁用自动注册

定义宏 `DisableLocalSaveServiceSDS`，可以禁止该包自动注册 `ISaveService`。
