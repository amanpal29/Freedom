using System.Collections.Generic;

namespace Freedom.Domain.Infrastructure
{
    public interface IIndexer<TObject, in TIndexer>
    {
        TObject this[TIndexer idx] { get; set; }
    }

    public interface IEnumerableIndexer<TObject, in TIndexer> : IIndexer<TObject, TIndexer>, IEnumerable<TObject>
    {
    }
}
