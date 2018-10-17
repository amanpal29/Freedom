using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.Model;

namespace Freedom.Domain.Infrastructure
{
    public class FreedomContextApplicationSettings : ApplicationSettingsBase, IAsyncInitializable
    {
        private readonly FreedomLocalContext _context;

        public FreedomContextApplicationSettings(FreedomLocalContext context)
        {
            _context = context;
        }

        public bool IsInitialized { get; set; }

        public Task InitializeCoreAsync()
        {
            return _context.ApplicationSetting.LoadAsync();
        }

        public override string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));

                if (!IsInitialized)
                    throw new InvalidOperationException("The settings collection has not been initialized yet.");

                ApplicationSetting setting = _context.ApplicationSetting.Local.FirstOrDefault(x => x.Key == key);

                return setting?.Value;
            }
            set
            {
                ApplicationSetting setting = _context.ApplicationSetting.Local.FirstOrDefault(x => x.Key == key);

                if (setting == null)
                {
                    setting = new ApplicationSetting();
                    setting.Key = key;
                    setting.Value = value;
                    _context.ApplicationSetting.Add(setting);
                }
                else
                {
                    setting.Value = value;
                }
            }
        }
    }
}
