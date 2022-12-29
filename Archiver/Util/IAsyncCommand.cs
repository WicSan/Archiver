using System.Threading.Tasks;
using System.Windows.Input;

namespace ArchivePlanner.Util
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}
