using FGUFW;
using UnityEngine;

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
