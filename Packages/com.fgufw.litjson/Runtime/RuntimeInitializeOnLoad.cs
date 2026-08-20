using FGUFW;
using UnityEngine;

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