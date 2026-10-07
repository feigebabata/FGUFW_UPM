using FGUFW;
using UnityEngine;

// 本程序集没有任何类型被游戏代码直接引用，只靠下面的 [RuntimeInitializeOnLoadMethod] 自注册。
// IL2CPP + Managed Stripping 会把“没人引用”的程序集整包裁掉，真机启动时 fg.toJson / fg.toObject 抛
// "IJsonService is not registered. Install one compatible service package..."。
// 用 AlwaysLinkAssembly 保证它一定参与托管代码裁剪流程、不被整包删除。
[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace LitJson
{
    public static class RuntimeInitializeOnLoad
    {
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void runtimeInitializeOnLoad()
        {
            #if !DisableLitJsonServiceSDS
            fg.RegisterJson(new LitJsonService());
            #endif
        }
    }
}
