// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using System;
// using System.Threading.Tasks;
// using UnityEngine.ResourceManagement.ResourceProviders;
// using UnityEngine.SceneManagement;
// using UnityEngine.ResourceManagement.AsyncOperations;
// using System.Threading;
// using Cysharp.Threading.Tasks;

// namespace FGUFW
// {
//     public static class AddressablesUtility
//     {
//         static IAssetLoader assetLoader = new Addressable_AssetLoader();
//         static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//         public static UniTask<T> LoadAsync<T>(string path)
//         {
//             return assetLoader.LoadAsync<T>(path,cancellationTokenSource.Token);
//         }

//         public static T Load<T>(string path)
//         {
//             return assetLoader.Load<T>(path);
//         }

//         public static UniTask<GameObject> InstantiateAsync(string path,Transform parent)
//         {
//             return assetLoader.InstantiateAsync(path,parent,cancellationTokenSource.Token);
//         }

//         public static GameObject Instantiate(string path,Transform parent)
//         {
//             return assetLoader.Instantiate(path,parent);
//         }

//         public static UniTask LoadSceneAsync(string path,LoadSceneMode loadSceneMode = LoadSceneMode.Single)
//         {
//             return assetLoader.LoadSceneAsync(path,loadSceneMode);
//         }

//         public static void ReleaseInstance(GameObject instance)
//         {
//             assetLoader.ReleaseInstance(instance);
//         }


//     }


// }
