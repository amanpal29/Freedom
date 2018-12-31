using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Freedom.Domain.Model;
using log4net;

namespace Freedom.Client.Infrastructure.LookupData
{
    internal class ApplicationDataLookupRepository : ILookupRepository
    {
        private const string LookupDataPath = "LookupData";

        private const int BufferSize = 1024*1024;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _folder;

        public ApplicationDataLookupRepository()
        {
            try
            {
#if DEBUG
                _folder = Path.Combine(App.DataFolder, LookupDataPath,
                    Assembly.GetExecutingAssembly().GetName().Version.ToString());
#else
                _folder = Path.Combine(App.DataFolder, LookupDataPath,
                    ApplicationSettings.Current.GlobalId.ToString());
#endif
                if (!Directory.Exists(_folder))
                {
                    Directory.CreateDirectory(_folder);
                }
            }
            catch (Exception exception)
            {
                Log.Warn("An error occurred while attempting to initialize the storage for LookupData", exception);
            }
        }

        private static DataContractSerializer GetSerializer<TEntity>()
        {
            IEnumerable<Type> knownTypes = Assembly.GetAssembly(typeof(EntityBase))
                    .GetTypes().Where(t => typeof(EntityBase).IsAssignableFrom(t));

            return new DataContractSerializer(typeof(LookupDataMemento<TEntity>), knownTypes);
        }

        public DateTime? LoadLookupData<TEntity>(ICollection<TEntity> lookupTable)
            where TEntity : EntityBase
        {
            try
            {
                string[] fileNames = Directory.GetFiles(_folder, $"{typeof (TEntity).Name}.xml");

                if (fileNames.Length <= 0)
                    return null;

                DataContractSerializer serializer = GetSerializer<TEntity>();

                using (FileStream stream = new FileStream(fileNames[0], FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
                {
                    LookupDataMemento<TEntity> memento = (LookupDataMemento<TEntity>) serializer.ReadObject(stream);

                    lookupTable.Clear();

                    foreach (TEntity entity in memento.Entities)
                        lookupTable.Add(entity);

                    return memento.RefreshedDateTime;
                }
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn($"An error occurred trying to load {typeof (TEntity).Name} LookupData from disk.", ex);

                return null;
            }
        }

        public bool SaveLookupData<TEntity>(DateTime refreshedDateTime, IEnumerable<TEntity> entities) where TEntity : EntityBase
        {
            try
            {
                LookupDataMemento<TEntity> memento = new LookupDataMemento<TEntity>(refreshedDateTime, entities);

                string filePath = Path.Combine(_folder, typeof (TEntity).Name + ".xml");

                DataContractSerializer serializer = GetSerializer<TEntity>();

                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize))
                {
                    serializer.WriteObject(stream, memento);
                    stream.Flush(true);
                    stream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warn($"An error occurred trying to save {typeof (TEntity).Name} LookupData to disk.", ex);

                return false;
            }
        }
    }
}
