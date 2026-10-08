using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGUFW
{
    /// <summary>
    /// 注意避免资源加载卸载互相等待导致的死锁情况 
    /// Addressables 文档明确指出：场景尚未完全加载时在 Awake 调用 WaitForCompletion，可能阻塞主线程，使 Bundle 卸载、场景集成等异步操作无法完成
    /// </summary>
    public interface IAssetLoaderService
    {
        T Load<T>(string path);

        Task<T> LoadAsync<T>(string path);

        GameObject Instantiate(string path, Transform parent);

        Task<GameObject> InstantiateAsync(string path,Transform parent);

        Task LoadSceneAsync(string path,LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        void ReleaseAsset(object asset);
        
        void ReleaseInstance(GameObject gameObject);
    }
}
