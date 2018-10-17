using System.Threading.Tasks;
using System.Windows.Input;

namespace Freedom
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}