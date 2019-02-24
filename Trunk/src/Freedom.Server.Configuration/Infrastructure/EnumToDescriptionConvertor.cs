using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Data;

namespace Freedom.Server.Tools.Infrastructure
{    
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum)
            {
                FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

                DescriptionAttribute descriptionAttribute = (DescriptionAttribute)
                    Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));

                if (descriptionAttribute != null)
                    return descriptionAttribute.Description;
            }

            return ToDisplayName(value);
        }

        private static string ToDisplayName(object obj)
        {
            return obj != null ? ToDisplayName(obj.ToString()) : null;
        }

        private static string ToDisplayName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            StringBuilder builder = new StringBuilder();

            int lastIndex = value.Length - 1;

            builder.Append(char.ToUpper(value[0]));

            for (int i = 1; i < lastIndex; i++)
            {
                if (char.IsUpper(value[i]) && char.IsLower(value[i + 1]))
                    builder.Append(' ');

                builder.Append(value[i]);
            }

            builder.Append(value[lastIndex]);

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
