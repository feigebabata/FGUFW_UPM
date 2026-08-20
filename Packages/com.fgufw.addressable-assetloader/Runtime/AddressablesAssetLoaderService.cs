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
            return GetOperationResult(operation, path);
        }

        /// <summary>
        /// 异步加载资源，资源使用结束后需要调用ReleaseAsset。
        /// </summary>
        public Task<T> LoadAsync<T>(string path)
        {
            ValidatePath(path);
            return ToTask(AddressablesAPI.LoadAssetAsync<T>(path), path);
        }

        /// <summary>
        /// 同步实例化对象，实例使用结束后需要调用ReleaseInstance。
        /// </summary>
        public GameObject Instantiate(string path, Transform parent)
        {
            ValidatePath(path);

            var operation = AddressablesAPI.InstantiateAsync(path, parent);
            operation.WaitForCompletion();
            return GetOperationResult(operation, path);
        }

        /// <summary>
        /// 异步实例化对象，实例使用结束后需要调用ReleaseInstance。
        /// </summary>
        public Task<GameObject> InstantiateAsync(string path, Transform parent)
        {
            ValidatePath(path);
            return ToTask(AddressablesAPI.InstantiateAsync(path, parent), path);
        }

        /// <summary>
        /// 加载Addressables场景。
        /// </summary>
        public async Task LoadSceneAsync(string path, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            ValidatePath(path);
            await ToTask(AddressablesAPI.LoadSceneAsync(path, loadSceneMode), path);
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
        /// 将Addressables操作转换为标准Task。
        /// </summary>
        private static Task<T> ToTask<T>(AsyncOperationHandle<T> operation, string path)
        {
            if (operation.IsDone)
            {
                try
                {
                    return Task.FromResult(GetOperationResult(operation, path));
                }
                catch (Exception exception)
                {
                    return Task.FromException<T>(exception);
                }
            }

            var completionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            operation.Completed += completedOperation =>
            {
                try
                {
                    completionSource.TrySetResult(GetOperationResult(completedOperation, path));
                }
                catch (Exception exception)
                {
                    completionSource.TrySetException(exception);
                }
            };
            return completionSource.Task;
        }

        /// <summary>
        /// 检查操作结果，失败时释放句柄并抛出原始异常。
        /// </summary>
        private static T GetOperationResult<T>(AsyncOperationHandle<T> operation, string path)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                return operation.Result;
            }

            var exception = operation.OperationException ?? new InvalidOperationException($"Addressables operation failed: {path}");
            if (operation.IsValid())
            {
                AddressablesAPI.Release(operation);
            }
            throw exception;
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
