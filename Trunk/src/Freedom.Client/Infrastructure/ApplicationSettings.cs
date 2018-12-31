using Freedom.Domain.Infrastructure;
using Freedom.Domain.Model;
using Freedom.Domain.Services.Command;
using Freedom.Domain.Services.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freedom.Client.Infrastructure
{
    public class ApplicationSettings : ApplicationSettingsBase
    {
        #region Fields

        private static ApplicationSettings _current;

        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        #endregion

        #region Singleton Constructor

        protected internal static void Reset()
        {
            _current = new ApplicationSettings();
        }

        public static void SetUpForUnitTesting()
        {
            Reset();
        }

        public static async Task InitializeAsync()
        {
            Reset();

            await _current.LoadAllSettingsAsync();
        }

        public static bool IsInitialized => _current != null;

        public static ApplicationSettings Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("ApplicationSettings has not been Initialized");

                return _current;
            }
        }

        #endregion

        #region Indexer

        public override string this[string key]
        {
            get { return _cache.ContainsKey(key) ? _cache[key] : null; }
            set
            {
                if (_cache.ContainsKey(key) && _cache[key] == value) return;
                _cache[key] = value;
            }
        }

        #endregion

        #region Persistance

        public async Task LoadAllSettingsAsync()
        {
            IEntityRepository entityRepository = IoC.TryGet<IEntityRepository>();

            if (entityRepository == null)
                return;

            List<ApplicationSetting> settings = await entityRepository.Get<ApplicationSetting>()
                .ToListAsync();

            _cache.Clear();

            foreach (ApplicationSetting setting in settings)
                _cache.Add(setting.Key, setting.Value);            
        }

        public async Task SaveAllSettingsAsync()
        {
            IEntityRepository entityRepository = IoC.Get<IEntityRepository>();
            ICommandService commandService = IoC.Get<ICommandService>();

            List<ApplicationSetting> currentSettings =
                await entityRepository.Get<ApplicationSetting>().ToListPagedAsync(1000);
                     

            foreach (KeyValuePair<string, string> keyValuePair in _cache)
            {
                ApplicationSetting setting = currentSettings.FirstOrDefault(x => x.Key == keyValuePair.Key);

                if (setting == null)
                {
                    setting = new ApplicationSetting();
                    setting.Key = keyValuePair.Key;
                }

                setting.Value = keyValuePair.Value;               
            }
           
        }

        #endregion
    }
}
