using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Sequence;
using Freedom.Domain.Services.Time;
using Freedom.TextBuilder;
using log4net;

namespace Freedom.Domain.Infrastructure
{
    [SuppressMessage("ReSharper", "CanBeReplacedWithTryCastAndCheckForNull")]
    public class AutoNumberGenerator : IAutoNumberGenerator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task ApplyAutoNumberingOnCreateAsync(ApplicationSettingsBase applicationSettings, NumberedRoot entity, User currentUser)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            AutoNumberSettings autoNumberSettings = GetAutoNumberSettingsForType(applicationSettings, entity.GetType());

            if (autoNumberSettings == null || autoNumberSettings.Mode != AutoNumberMode.OnCreate)
                return;

            entity.Number = await GenerateFormattedNumber(null, autoNumberSettings, entity, currentUser);
        }
        
        public async Task ApplyAutoNumberingOnCommitAsync(FreedomLocalContext localContext, ApplicationSettingsBase applicationSettings, NumberedRoot entity, User currentUser)
        {
            AutoNumberSettings autoNumberSettings = GetAutoNumberSettingsForType(applicationSettings, entity.GetType());

            if (autoNumberSettings == null || autoNumberSettings.Mode != AutoNumberMode.OnCommit)
                return;

            if (!string.IsNullOrWhiteSpace(entity.Number) && autoNumberSettings.AllowManualOverride)
                return;

            entity.Number = await GenerateFormattedNumber(localContext, autoNumberSettings, entity, currentUser);
        }

        public AutoNumberSettings GetAutoNumberSettingsForType(ApplicationSettingsBase applicationSettings, Type entityType)
        {
            if (applicationSettings == null)
                throw new ArgumentNullException(nameof(applicationSettings));

            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            string key = GetTypeKey(entityType) + "AutoNumberSettings";

            string value = applicationSettings[key];

            return !string.IsNullOrWhiteSpace(value) ? AutoNumberSettings.FromString(value) : null;
        }

        private static async Task<string> GenerateFormattedNumber(FreedomLocalContext localContext, AutoNumberSettings settings, EntityBase entity, User currentUser)
        {
            if (string.IsNullOrEmpty(settings.Format))
                return null;

            try
            {
                DataProxy dataProxy = new DataProxy(localContext, entity, currentUser);

                await dataProxy.InitializeAsync(TextBuilder.TextBuilder.GetFieldList(settings.Format));

                string result = TextBuilder.TextBuilder.Format(settings.Format, dataProxy);

                Log.Debug($"Auto-Generated new document number '{result}' for '{entity}'");

                return result;
            }
            catch (TextBuilderException e)
            {
                string message =
                    $"Unable to generate auto-generated number for entity {entity} due to the following errors in the configuration: {string.Join(";", e.Errors)}";

                Log.Warn(message, e);

                return null;
            }
        }

        private static string GetTypeKey(Type entityType)
        {
           // if (typeof (Contact).IsAssignableFrom(entityType))
           //     return "Contact";            
            return null;
        }

        internal class DataProxy
        {
            private readonly EntityBase _entity;
            private readonly User _user;
            private readonly FreedomLocalContext _db;

            internal DataProxy(FreedomLocalContext db, EntityBase entity, User currentUser)
            {
                _entity = entity;
                _user = currentUser;
                _db = db;
            }

            public async Task InitializeAsync(ICollection<string> fields)
            {
                if (fields == null || fields.Count == 0)
                    return;
                
                if (fields.Contains(nameof(UniqueNumber)))
                {
                    ISequenceGenerator sequenceGenerator = IoC.Get<ISequenceGenerator>();

                    string key = GetTypeKey(_entity.GetType());

                    UniqueNumber = await sequenceGenerator.GetNextValueAsync(key);
                }
            }
                        
            public DateTime CurrentDate
            {
                get
                {
                    ITimeService timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

                    return timeService.UtcNow.ToLocalTime();
                }
            }

            public long UniqueNumber
            {
                get; private set;
            }

            
        }
    }
}
