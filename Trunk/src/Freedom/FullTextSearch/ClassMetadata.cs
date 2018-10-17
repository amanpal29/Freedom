using System;
using System.ComponentModel;
using System.Reflection;

namespace Freedom.FullTextSearch
{
    public sealed class ClassMetadata<T>
    {
        #region Singleton

        private static readonly Lazy<ClassMetadata<T>> Lazy =
            new Lazy<ClassMetadata<T>>(() => new ClassMetadata<T>());

        #endregion

        #region Fields

        private readonly PropertyMetadataDictionary<T> _properties = new PropertyMetadataDictionary<T>();

        #endregion

        #region Methods

        private static bool IsIndexablePropertyType(Type type)
        {
            IndexHintAttribute indexHintAttribute = type.GetCustomAttribute<IndexHintAttribute>();

            if (indexHintAttribute != null && indexHintAttribute.IndexHints.HasFlag(IndexHints.Force))
                return true;

            return type == typeof(string) || type.IsEnum;
        }

        #endregion

        #region Singleton Constructor

        private ClassMetadata()
        {
            foreach (PropertyDescriptor property in  TypeDescriptor.GetProperties(typeof(T)))
            {
                PropertyMetadata<T> propertyMetadata = new PropertyMetadata<T>(property);

                if (!propertyMetadata.IndexHint.HasFlag(IndexHints.Force))
                {
                    if (propertyMetadata.IndexHint.HasFlag(IndexHints.Ignore)) continue;

                    if (!property.IsBrowsable) continue;

                    if (!IsIndexablePropertyType(propertyMetadata.PropertyType)) continue;
                }

                _properties.Add(propertyMetadata.Name, propertyMetadata);
            }
        }

        #endregion

        #region Properties

        public static PropertyMetadataDictionary<T> Properties => Lazy.Value._properties;

        #endregion
    }
}