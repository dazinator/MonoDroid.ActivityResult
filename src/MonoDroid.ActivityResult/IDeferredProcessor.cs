using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public interface IDeferredProcessor
    {
        Task ProcessResults();
    }
}

