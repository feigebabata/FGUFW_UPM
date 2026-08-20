using System;
using UnityEngine;

namespace FGUFW
{
    public static partial class fg
    {

        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetServices()
        {
            registeredAssetLoader = null;
            registeredJsonService = null;
            registeredSaveService = null;
        }

        private static IAssetLoaderService registeredAssetLoader;
        public static IAssetLoaderService assetLoader => registeredAssetLoader ?? throw CreateMissingServiceException(nameof(IAssetLoaderService));

        public static void RegisterAssetLoader(IAssetLoaderService service)
        {
            RegisterService( ref registeredAssetLoader, service, nameof(IAssetLoaderService));
        }

        private static IJsonService registeredJsonService;
        private static IJsonService jsonService => registeredJsonService ?? throw CreateMissingServiceException( nameof(IJsonService));

        public static void RegisterJson(IJsonService service)
        {
            RegisterService( ref registeredJsonService, service, nameof(IJsonService));
        }

        public static string toJson(object obj)
        {
            return jsonService.ToJson(obj);
        }

        public static T toObject<T>(string jsonText)
        {
            return jsonService.ToObject<T>(jsonText);
        }

        private static ISaveService registeredSaveService;
        public static ISaveService save => registeredSaveService ?? throw CreateMissingServiceException(nameof(ISaveService));

        public static void RegisterSave(ISaveService service)
        {
            RegisterService( ref registeredSaveService, service, nameof(ISaveService));
        }


        private static void RegisterService<T>( ref T current, T incoming, string serviceName) where T : class
        {
            if (incoming == null)
            {
                throw new ArgumentNullException( nameof(incoming), $"{serviceName} cannot be null.");
            }

            if (current == null || ReferenceEquals(current, incoming))
            {
                current = incoming;
                return;
            }

            throw new InvalidOperationException( $"{serviceName} is already registered as " + $"{current.GetType().FullName}. Cannot register " + $"{incoming.GetType().FullName}.");
        }

        private static InvalidOperationException CreateMissingServiceException( string serviceName)
        {
            return new InvalidOperationException( $"{serviceName} is not registered. Install one compatible " + $"service package or register an implementation before use.");
        }
    }
}
