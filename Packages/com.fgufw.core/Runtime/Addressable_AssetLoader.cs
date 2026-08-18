// using System.Threading;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
// using UnityEngine.SceneManagement;

// namespace FGUFW
// {
//     public sealed class Addressable_AssetLoader : IAssetLoader
//     {
//         public T Load<T>(string path)
//         {
//             return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
//         }

//         public async UniTask<T> LoadAsync<T>(string path, CancellationToken cancellationToken)
//         {
//             return await Addressables.LoadAssetAsync<T>(path).WithCancellation(cancellationToken);
//         }
//         public GameObject Instantiate(string path, Transform parent)
//         {
//             return Addressables.InstantiateAsync(path,parent).WaitForCompletion();
//         }

//         public async UniTask<GameObject> InstantiateAsync(string path, Transform parent, CancellationToken cancellationToken)
//         {
//             return await Addressables.InstantiateAsync(path,parent).WithCancellation(cancellationToken);
//         }


//         public UniTask LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
//         {
//             return Addressables.LoadSceneAsync(path,loadSceneMode).ToUniTask();
//         }

//         public void ReleaseInstance(GameObject instance)
//         {
//             Addressables.ReleaseInstance(instance);
//         }

//     }
// }
