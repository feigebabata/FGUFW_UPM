using System;
using System.Threading.Tasks;
using FGUFW;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using AddressablesAPI = UnityEngine.AddressableAssets.Addressables;

namespace FGUFW.AddressablesAssetLoader
{
    /// <summary>
    /// 使用Unity Addressables实现资源加载服务。
    /// </summary>
    public sealed class AddressablesAssetLoaderService : IAssetLoaderService
    {
        /// <summary>
        /// 同步加载资源，资源使用结束后需要调用ReleaseAsset。
        /// </summary>
        public T Load<T>(string path)
        {
            ValidatePath(path);

            var operation = AddressablesAPI.LoadAssetAsync<T>(path);
            operation.WaitForCompletion();
            return operation.Result;
        }

        /// <summary>
        /// 异步加载资源，资源使用结束后需要调用ReleaseAsset。
        /// </summary>
        public Task<T> LoadAsync<T>(string path)
        {
            ValidatePath(path);

            var operation =  AddressablesAPI.LoadAssetAsync<T>(path);
            return operation.Task;
        }

        /// <summary>
        /// 同步实例化对象，实例使用结束后需要调用ReleaseInstance。
        /// </summary>
        public GameObject Instantiate(string path, Transform parent)
        {
            ValidatePath(path);

            var operation = AddressablesAPI.InstantiateAsync(path, parent);
            operation.WaitForCompletion();
            return operation.Result;
        }

        /// <summary>
        /// 异步实例化对象，实例使用结束后需要调用ReleaseInstance。
        /// </summary>
        public Task<GameObject> InstantiateAsync(string path, Transform parent)
        {
            ValidatePath(path);
            var operation = AddressablesAPI.InstantiateAsync(path, parent);
            return operation.Task;
        }

        /// <summary>
        /// 加载Addressables场景。
        /// </summary>
        public async Task LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            ValidatePath(path);
            var operation = AddressablesAPI.LoadSceneAsync(path, loadSceneMode);
            await operation.Task;
        }

        /// <summary>
        /// 释放通过Load或LoadAsync加载的资源。
        /// </summary>
        public void ReleaseAsset(object asset)
        {
            if (asset != null)
            {
                AddressablesAPI.Release(asset);
            }
        }

        /// <summary>
        /// 释放通过Instantiate或InstantiateAsync创建的实例。
        /// </summary>
        public void ReleaseInstance(GameObject gameObject)
        {
            if (gameObject != null)
            {
                AddressablesAPI.ReleaseInstance(gameObject);
            }
        }

        /// <summary>
        /// 检查资源地址。
        /// </summary>
        private static void ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Addressables path cannot be empty.", nameof(path));
            }
        }
    }
}
