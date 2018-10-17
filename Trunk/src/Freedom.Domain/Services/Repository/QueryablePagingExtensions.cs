using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.Repository
{
    public static class QueryablePagingExtensions
    {
        public static async Task<List<TSource>> ToListPagedAsync<TSource>(this IQueryable<TSource> source, int pageSize)
        {
            List<TSource> result = await source.Take(pageSize).ToListAsync();

            if (result.Count < pageSize)
                return result;

            int total = source.Count();

            while (result.Count < total)
            {
                List<TSource> page = await source.Skip(result.Count).Take(pageSize).ToListAsync();

                result.AddRange(page);

                if (page.Count < pageSize)
                    break;
            }

            return result;
        }
    }
}
