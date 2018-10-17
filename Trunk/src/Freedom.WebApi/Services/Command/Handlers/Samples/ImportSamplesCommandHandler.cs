using System.Collections.Generic;
using System.Threading.Tasks;
using Hedgehog.CommandModel.Samples;
using Hedgehog.Model;

namespace Hedgehog.Services.Command.Handlers.Samples
{
    public class ImportSamplesCommandHandler : RepositoryCommandHandler<ImportSamplesCommand>
    {
        public override IEnumerable<SystemPermission> RequiredPermissions
        {
            get { yield break; }
        }

        protected override Task<bool> Handle(HedgehogRepository repository, ImportSamplesCommand command, CommandExecutionContext context)
        {
            foreach (Sample sample in command.Samples)
                repository.Add(sample);
            
            return Task.FromResult(true);
        }
    }
}
