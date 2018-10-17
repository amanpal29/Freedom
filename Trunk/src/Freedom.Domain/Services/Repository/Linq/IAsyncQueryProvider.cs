using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.Repository.Linq
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<object> ExecuteAsync(Expression expression);

        Task<TResult> ExecuteAsync<TResult>(Expression expression);
    }
}
