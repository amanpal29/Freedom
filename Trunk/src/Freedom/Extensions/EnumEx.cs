using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Freedom.Extensions
{
    public static class EnumEx
    {
        private static FieldInfo GetEnumField<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException("TEnum must be an enum type");

            return typeof(TEnum).GetField(enumValue.ToString());
        }

        public static T Parse<T>(string value)
        {
            return (T) Enum.Parse(typeof (T), value, true);
        }

        public static bool IsBrowsable<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            FieldInfo field = GetEnumField(enumValue);

            BrowsableAttribute browsableAttribute = field.GetCustomAttribute<BrowsableAttribute>(false);

            return browsableAttribute == null || browsableAttribute.Browsable;
        }

        public static string GetCategory<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            FieldInfo field = GetEnumField(enumValue);

            CategoryAttribute categoryAttribute = field.GetCustomAttribute<CategoryAttribute>(false);

            return categoryAttribute?.Category;
        }

        public static string GetDisplayName<TEnum>(TEnum enumValue)
            where TEnum : struct
        {
            FieldInfo field = GetEnumField(enumValue);

            DescriptionAttribute descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();

            return descriptionAttribute != null
                ? descriptionAttribute.Description
                : enumValue.ToString().ToDisplayName();
        }

        public static IEnumerable<KeyValuePair<object, string>> GetValueDisplayNamePairs(Type enumType)
        {
            foreach (object enumValue in Enum.GetValues(enumType))
            {
                FieldInfo fieldInfo = enumType.GetField(enumValue.ToString());

                if (fieldInfo.GetCustomAttribute<BrowsableAttribute>()?.Browsable == false) continue;

                DisplayNameAttribute displayNameAttribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();

                string displayName = displayNameAttribute != null
                    ? displayNameAttribute.DisplayName
                    : enumValue.ToString().ToDisplayName();

                yield return new KeyValuePair<object, string>(enumValue, displayName);
            }
        }

        public static IEnumerable<KeyValuePair<TEnum, string>> GetValueDisplayNamePairs<TEnum>()
            where TEnum : struct
        { 
            foreach (TEnum enumValue in Enum.GetValues(typeof(TEnum)))
            {
                FieldInfo fieldInfo = typeof(TEnum).GetField(enumValue.ToString());

                if (fieldInfo.GetCustomAttribute<BrowsableAttribute>()?.Browsable == false) continue;

                DisplayNameAttribute displayNameAttribute = fieldInfo.GetCustomAttribute<DisplayNameAttribute>();

                string displayName = displayNameAttribute != null
                    ? displayNameAttribute.DisplayName
                    : enumValue.ToString().ToDisplayName();

                yield return new KeyValuePair<TEnum, string>(enumValue, displayName);
            }
        }
    }
}
