using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FGUFW
{
    public interface IAssetLoader
    {
        T Load<T>(string path);
        Task<T> LoadAsync<T>(string path,CancellationToken cancellationToken);
        GameObject Instantiate(string path,Transform parent);
        Task<GameObject> InstantiateAsync(string path,Transform parent,CancellationToken cancellationToken);
        Task LoadSceneAsync(string path,LoadSceneMode loadSceneMode = LoadSceneMode.Single);
        void ReleaseInstance(GameObject game);
    }
}
