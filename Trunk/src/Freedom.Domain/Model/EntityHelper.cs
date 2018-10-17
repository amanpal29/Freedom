using System.IO;
using System.Runtime.Serialization;

namespace Freedom.Domain.Model
{
    public static class EntityHelper
    {
        public static string FormatNameAndNumber(string name, string number)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(number))
                return name;

            return $"{name} [{number}]";
        }

        public static TEntity Clone<TEntity>(this TEntity entity)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(TEntity));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, entity);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return (TEntity) serializer.ReadObject(memoryStream);
            }
        }
    }
}
