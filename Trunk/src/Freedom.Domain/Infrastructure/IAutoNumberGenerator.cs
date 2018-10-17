using System;
using System.Threading.Tasks;
using Freedom.Domain.Model;

namespace Freedom.Domain.Infrastructure
{
    public interface IAutoNumberGenerator
    {
        Task ApplyAutoNumberingOnCreateAsync(ApplicationSettingsBase applicationSettings, NumberedRoot entity, User currentUser);

        Task ApplyAutoNumberingOnCommitAsync(FreedomLocalContext localContext, ApplicationSettingsBase applicationSettings, NumberedRoot entity, User currentUser);

        AutoNumberSettings GetAutoNumberSettingsForType(ApplicationSettingsBase applicationSettings, Type entityType);
    }
}