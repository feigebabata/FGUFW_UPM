# Gameplay

- 管理业务逻辑 树形结构
- 一个Play(GameObject)下挂载其他Part(GameObject)并递归

## Part
- 业务功能实现单元
- 继承MonoBehaviour方便Editor调试
- 通过实例化预制件方式加载 可以直接在预制件绑定资源
- 接口AddPart<T>(string partPrefabPath=default) 当partPrefabPath为空时 调用T转partPrefabPath的功能
- 禁止相同Part并列
- 接口OnInitCheckParts 获取自身节点下的所有Part 并调用AddPart(Part part)
- 接口和字段:
  - bool PartEnabled
  - void OnInitCheckParts()
  - void OnCreatedPart()
  - void OnDestroyPart()
  - void OnEnablePart()
  - void OnDisablePart()
  - void AddPart(Part part)
  - void AddPart<T>(string partPrefabPath=default) //结束调用OnCreatePart
  - T GetPart<T>()
  - void RemovePart<T>() //提前调用OnDestroyPart

## Play
- 基础Part
- 自身是单例 但不会自动创建
- 简单的获取Part方式 : T part = Play.I.GetPart<A>().GetPart<T>()
- 在初始化自动调用 OnCreatePart

