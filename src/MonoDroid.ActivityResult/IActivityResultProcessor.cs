using System.Threading.Tasks;

namespace MonoDroid.ActivityResult
{
    public interface IActivityResultProcessor
    {
        Task ProcessResults();
    }
}

