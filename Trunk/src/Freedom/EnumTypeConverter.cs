using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Freedom
{
    public class EnumTypeConverter : EnumConverter
    {
        public EnumTypeConverter(Type enumType) : base(enumType){}

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null)
            {
                Type enumType = value.GetType();
                if (enumType.IsEnum)
                    return GetDescription(value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        private string GetDescription(object enumValue)
        {
            DescriptionAttribute descriptionAttribute =
                EnumType.GetField(enumValue.ToString())
                    .GetCustomAttributes(typeof (DescriptionAttribute), false)
                    .FirstOrDefault() as DescriptionAttribute;

            return descriptionAttribute != null ? descriptionAttribute.Description : Enum.GetName(EnumType, enumValue);
        }
    }
}
