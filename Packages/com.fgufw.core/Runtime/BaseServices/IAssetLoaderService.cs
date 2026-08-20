using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGUFW
{
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
