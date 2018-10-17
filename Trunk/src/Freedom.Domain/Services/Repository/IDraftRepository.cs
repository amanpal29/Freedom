using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Freedom.Domain.Services.Repository
{
    public interface IDraftRepository
    {
        Task<WorkflowMemento> GetMementoAsync(Guid id);
        
        Task<IEnumerable<WorkflowMemento>> GetMementosAsync(string typeName);

        Task<int> GetMementoCountAsync();

        Task<bool> SaveAutoSaveAsync(WorkflowMemento workflow);

        Task<bool> SaveDraftAsync(WorkflowMemento workflow);

        Task<bool> DeleteMementoAsync(Guid id);

        Task<bool> DeleteMementosAsync();

        event EventHandler RepositoryChanged;
    }
}
