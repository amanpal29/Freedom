using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using Freedom.Extensions;

namespace Freedom.Client.Infrastructure.LookupData
{
    public class EnumDataProvider : DataSourceProvider
    {
        private Type _enumType;

        public EnumDataProvider()
        {
        }

        public EnumDataProvider(Type enumType)
        {
            _enumType = enumType;
        }

        public Type EnumType
        {
            get { return _enumType; }
            set
            {
                if (_enumType != value)
                {
                    if (_enumType != null)
                        throw new InvalidOperationException("Once set, EnumType can't be changed.");

                    if (value != null && !value.IsEnum)
                        throw new ArgumentException("EnumType must be an enum.");

                    _enumType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("EnumType"));
                }
            }
        }

        protected override void BeginQuery()
        {
            List<KeyValuePair<object, string>> result = new List<KeyValuePair<object, string>>();

            if (_enumType != null && _enumType.IsEnum)
            {
                foreach (object enumValue in Enum.GetValues(_enumType))
                {
                    string name = Enum.GetName(_enumType, enumValue);

                    string description = name.ToDisplayName(); 

                    FieldInfo fieldInfo = _enumType.GetField(name);

                    if (fieldInfo != null)
                    {
                        BrowsableAttribute browsableAttribute = (BrowsableAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(BrowsableAttribute));

                        if (browsableAttribute != null && !browsableAttribute.Browsable)
                            continue;

                        DescriptionAttribute descriptionAttribute =
                            (DescriptionAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof (DescriptionAttribute));

                        if (descriptionAttribute != null)
                            description = descriptionAttribute.Description;
                    }

                    result.Add(new KeyValuePair<object, string>(enumValue, description));
                }
            }

            OnQueryFinished(result, null, null, null);
        }
    }
}
