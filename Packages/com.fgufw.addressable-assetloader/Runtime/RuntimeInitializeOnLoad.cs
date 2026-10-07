using FGUFW;
using UnityEngine;

// 本程序集没有任何类型被游戏代码直接引用，只靠下面的 [RuntimeInitializeOnLoadMethod] 自注册。
// IL2CPP + Managed Stripping 会把“没人引用”的程序集整包裁掉，真机启动时 fg.assetLoader 抛
// "IAssetLoaderService is not registered. Install one compatible service package..."。
// AlwaysLinkAssembly 是 Unity 官方给这种“只被引擎回调/反射用到”的程序集准备的口子：
// 保证它一定参与托管代码裁剪流程，不会因为没有任何引用而被整个删除。（参见 UnityEngine.Scripting.AlwaysLinkAssemblyAttribute）
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace FGUFW.AddressablesAssetLoader
{
    public static class RuntimeInitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void RegisterService()
        {
#if !DisableAddressablesAssetLoaderServiceSDS
            fg.RegisterAssetLoader(new AddressablesAssetLoaderService());
#endif
        }
    }
}
