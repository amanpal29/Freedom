using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Freedom.FullTextSearch
{
    public class IndexBuilder : IDisposable
    {
        private const int BaseWeight = 0x10000;

        private readonly IndexRepository _repository;

        public IndexBuilder(DbConnection connection)
        {
            _repository = new IndexRepository(connection);

            PositionalWeightFactor = 0.9;

            PartialMatchAdjustment = 0.8;

            MinimumPartialMatchLength = 3;

            MaximumKeywordLength = 32;

            WordBreaker = new StandardEnglishWordBreaker();
        }

        public double PositionalWeightFactor { get; set; }

        public double PartialMatchAdjustment { get; set; }

        public int MinimumPartialMatchLength { get; set; }

        public int MaximumKeywordLength { get; set; }

        public IWordBreaker WordBreaker { get; set; }

        private static void Add(IDictionary<string, int> index, string word, int weight)
        {
            if (index.ContainsKey(word))
                index[word] += weight;
            else
                index.Add(word, weight);
        }

        private Dictionary<string, int> BuildIndex<TEntity>(TEntity entity)
        {
            Dictionary<string, int> index = new Dictionary<string, int>();

            foreach (PropertyMetadata<TEntity> property in ClassMetadata<TEntity>.Properties.Values)
            {
                string value = property.GetValue(entity);

                if (string.IsNullOrEmpty(value)) continue;

                double weight = BaseWeight * property.SearchWeight;

                string[] words = WordBreaker.BreakText(value);

                foreach (string baseWord in words)
                {
                    if (string.IsNullOrEmpty(baseWord)) continue;

                    string word = baseWord.Length > MaximumKeywordLength
                        ? baseWord.Substring(0, MaximumKeywordLength)
                        : baseWord;

                    Add(index, word, (int)weight);

                    for (int length = word.Length - 1; length >= MinimumPartialMatchLength; length--)
                        Add(index, word.Substring(0, length), (int) (weight*PartialMatchAdjustment));

                    weight *= PositionalWeightFactor;
                }
            }

            return index;
        }

        public async Task UpdateIndexAsync<TEntity>(IEnumerable<TEntity> entities, Func<TEntity, Guid> primaryKey, CancellationToken cancellationToken)
        {
            foreach (TEntity entity in entities)
            {
                if (cancellationToken.IsCancellationRequested) break;

                await UpdateIndexAsync(primaryKey(entity), entity, cancellationToken);
            }
        }

        public Task UpdateIndexAsync<TEntity>(Guid id, TEntity entity, CancellationToken cancellationToken)
        {
            return _repository.UpdateAsync(typeof(TEntity).Name.GetHashCode(), id, BuildIndex(entity), cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            _repository.Dispose();
        }
    }
}
