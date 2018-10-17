using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom;
using Freedom.Extensions;

namespace Freedom.Domain.Interfaces
{
    public interface IVersionedModelCache<out TModel> : IRefreshable 
        where TModel : AggregateRoot 
    {
        Guid? GetModelIdForParent(Guid? repositoryId);

        TModel this[Guid? key] { get; }

        Task LoadModelAsync(Guid modelId);

        Task LoadModelForParentAsync(Guid repositoryId);

        Task LoadModelForParentManyAsync(IEnumerable<Guid> repositoryIds);
    }
    
    public static class ModelCache<TModel> 
        where TModel : AggregateRoot
    {
        public static IVersionedModelCache<TModel> Instance => IoC.Get<IVersionedModelCache<TModel>>();

        public static Task RefreshAsync()
        {
            return Instance.RefreshAsync();
        }
        
        public static Guid? GetModelIdForParent(Guid? repositoryId)
        {
            return Instance.GetModelIdForParent(repositoryId);
        }
        
        public static TModel GetModelForParent(Guid? repositoryId)
        {
            IVersionedModelCache<TModel> instance = Instance;

            return instance[instance.GetModelIdForParent(repositoryId)];
        }
        
        public static TModel Get(Guid? key)
        {
            if (!key.HasValue)
                return null;

            TModel result = Instance[key];

            if (result == null)
                throw new KeyNotFoundException(
                    $"The key {key} was not found in the lookup for {typeof(TModel).Name.ToDisplayName()}");

            return result;
        }
        
        public static Task LoadModelAsync(Guid modelId)
        {
            return Instance.LoadModelAsync(modelId);
        }

        public static Task LoadModelForParentAsync(Guid repositoryId)
        {
            return Instance.LoadModelForParentAsync(repositoryId);
        }

        public static Task LoadModelForParentManyAsync(IEnumerable<Guid> repositoryIds)
        {
            return Instance.LoadModelForParentManyAsync(repositoryIds);
        }
    }
}
