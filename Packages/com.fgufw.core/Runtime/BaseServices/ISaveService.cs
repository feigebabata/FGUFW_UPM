using System.Threading.Tasks;

namespace FGUFW
{
    public interface ISaveService
    {
        Task LoadAsync();

        T Get<T>() where T : class, new();

        Task SaveAsync();
    }
}
