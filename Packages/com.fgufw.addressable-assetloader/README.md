# FGUFW Addressables Asset Loader

基于 Unity Addressables 的 `IAssetLoaderService` 实现。

## Packages\manifest.json
```
"com.fgufw.addressable-assetloader": "https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.addressable-assetloader#addressable-assetloader@1.0.0"
```

## 特性

- 安装后自动注册为 `fg.assetLoader`
- 支持同步与异步资源加载
- 支持同步与异步实例化
- 支持场景加载
- 支持普通资源与实例对象释放
- 不依赖 UniTask

## 使用

```csharp
var prefab = await fg.assetLoader.LoadAsync<GameObject>("MenuPanel");
var instance = await fg.assetLoader.InstantiateAsync("MenuPanel", null);

fg.assetLoader.ReleaseAsset(prefab);
fg.assetLoader.ReleaseInstance(instance);
```

同步接口通过 `WaitForCompletion` 阻塞当前线程，只适合已经位于本地或缓存中的资源。远程 Bundle 请使用异步接口。

## 禁用自动注册

定义宏 `DisableAddressablesAssetLoaderServiceSDS`，可以禁止该包自动注册 `IAssetLoaderService`。

安装地址：

```text
https://github.com/feigebabata/FGUFW_UPM.git?path=/Packages/com.fgufw.addressable-assetloader#addressable-assetloader@1.0.0
```
