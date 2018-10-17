using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using Freedom.Extensions;

namespace Freedom
{
    public static class RefelectionHelper
    {
        const BindingFlags Bindings = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private static void ResolvePath(object baseObject, string path, out object targetObject, out MemberInfo targetMember)
        {
            targetObject = baseObject;
            targetMember = null;

            object nextObject = baseObject;

            foreach (string memberName in path.Split('.'))
            {
                targetObject = nextObject;

                if (targetObject == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to set property {path} on entity of type {baseObject.GetType().FullName}, one of the objects in the path is null.");
                }

                Type objType = targetObject.GetType();

                targetMember = (MemberInfo) objType.GetProperty(memberName, Bindings) ??
                               objType.GetField(memberName, Bindings);

                if (targetMember == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to find field or property {memberName} on object of type {targetObject.GetType().FullName} when loading properties for {baseObject.GetType().Name} with path {path}.");
                }

                nextObject = GetValue(targetObject, targetMember);
            }
        }

        private static bool TryStringToEnum(Type enumType, string value, out Enum result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default(Enum);
                return false;
            }

            value = value.Trim();

            // First just check if this is a valid value for the enum and if so return it.

            if (Enum.IsDefined(enumType, value))
            {
                result = (Enum) Enum.Parse(enumType, value);
                return true;
            }

            string[] enumValues = Enum.GetValues(enumType).Cast<object>().Select(obj => obj.ToString()).ToArray();

            // Second, check if it is a valid value if we ignore case

            string[] matches = enumValues.Where(p => string.Compare(p, value, StringComparison.InvariantCultureIgnoreCase) == 0).ToArray();

            if (matches.Length == 1)
            {
                result = (Enum) Enum.Parse(enumType, matches[0]);
                return true;
            }

            // Third, check if there's is (only one) valid value that starts with this value

            matches = enumValues.Where(p => p.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)).ToArray();

            if (matches.Length == 1)
            {
                result = (Enum)Enum.Parse(enumType, matches[0]);
                return true;
            }

            // Conversion Failed

            result = default(Enum);
            return false;
        }

        public static object GetValue(object baseObject, MemberInfo memberInfo)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (!(memberInfo is PropertyInfo || memberInfo is FieldInfo))
                throw new ArgumentException("memberInfo must be an instance of FieldInfo or PropertyInfo", nameof(memberInfo));

            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            FieldInfo fieldInfo = memberInfo as FieldInfo;

            return propertyInfo != null
                ? propertyInfo.GetValue(baseObject, null)
                : fieldInfo.GetValue(baseObject);
        }

        public static void SetValue(object baseObject, MemberInfo memberInfo, object value)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            if (memberInfo is PropertyInfo)
            {
                ((PropertyInfo) memberInfo).SetValue(baseObject, value, null);
            }
            else if (memberInfo is FieldInfo)
            {
                ((FieldInfo) memberInfo).SetValue(baseObject, value);
            }
            else
            {
                throw new ArgumentException("memberInfo must be an instance of FieldInfo or PropertyInfo", nameof(memberInfo));
            }
        }

        public static Type GetMemberType(MemberInfo memberInfo)
        {
            if (memberInfo == null)
                throw new ArgumentNullException(nameof(memberInfo));

            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            
            if (propertyInfo != null)
                return propertyInfo.PropertyType;

            FieldInfo fieldInfo = memberInfo as FieldInfo;

            if (fieldInfo != null)
                return fieldInfo.FieldType;

            throw new ArgumentException("memberInfo must be an instance of FieldInfo or PropertyInfo", nameof(memberInfo));
        }

        public static Type GetMemberType(Type baseType, string path)
        {
            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            Type currentType = baseType;
            
            foreach (string memberName in path.Split('.'))
            {
                MemberInfo member = (MemberInfo) currentType.GetProperty(memberName, Bindings) ??
                                          currentType.GetField(memberName, Bindings);

                if (member == null)
                    throw new InvalidOperationException(
                        $"Unable to find field or property {memberName} on object of type {currentType.FullName} when loading properties for {baseType.FullName} with path {path}.");

                currentType = GetMemberType(member);
            }

            return currentType;
        }

        public static object GetValue(object baseObject, string path)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            string[] propertyNames = path.Split(".".ToCharArray());

            object obj = baseObject;

            foreach (string propertyName in propertyNames)
            {
                if (obj == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to set property {path} on object of type {baseObject.GetType().FullName}, one of the properties in the path is null.");
                }

                Type objType = obj.GetType();

                MemberInfo memberInfo = (MemberInfo) objType.GetProperty(propertyName, Bindings) ??
                                        objType.GetField(propertyName, Bindings);

                if (memberInfo == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to find property {propertyName} on object of type {objType.FullName} when loading properties for {baseObject.GetType().FullName} with path {path}.");
                }

                obj = GetValue(obj, memberInfo);
            }

            return obj;
        }

        public static void SetValue(object baseObject, string path, object value)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            object targetObject;
            MemberInfo targetMember;

            ResolvePath(baseObject, path, out targetObject, out targetMember);

            SetValue(targetObject, targetMember, value);
        }

        public static string ToString(object value)
        {
            if (value == null)
                return null;

            Type valueType = value.GetType();

            switch (Type.GetTypeCode(valueType))
            {
                case TypeCode.Boolean:
                    return XmlConvert.ToString((bool)value);

                case TypeCode.Char:
                    return XmlConvert.ToString((char)value);

                case TypeCode.SByte:
                    return XmlConvert.ToString((sbyte)value);

                case TypeCode.Byte:
                    return XmlConvert.ToString((byte)value);

                case TypeCode.Int16:
                    return XmlConvert.ToString((short)value);

                case TypeCode.UInt16:
                    return XmlConvert.ToString((ushort)value);

                case TypeCode.Int32:
                    return value is Enum ? value.ToString() : XmlConvert.ToString((int) value);
 
                case TypeCode.UInt32:
                    return XmlConvert.ToString((uint)value);

                case TypeCode.Int64:
                    return XmlConvert.ToString((long)value);

                case TypeCode.UInt64:
                    return XmlConvert.ToString((ulong)value);

                case TypeCode.Single:
                    return XmlConvert.ToString((float)value);

                case TypeCode.Double:
                    return XmlConvert.ToString((double)value);

                case TypeCode.Decimal:
                    return XmlConvert.ToString((decimal)value);

                case TypeCode.DateTime:
                    return XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);

                case TypeCode.String:
                    return (string)value;

                default:
                    if (valueType == typeof(TimeSpan))
                        return XmlConvert.ToString((TimeSpan)value);

                    if (valueType == typeof(Guid))
                        return XmlConvert.ToString((Guid)value);

                    if (valueType == typeof(DateTimeOffset))
                        return XmlConvert.ToString((DateTimeOffset)value);

                    if (valueType == typeof(byte[]))
                        return Convert.ToBase64String((byte[])value, Base64FormattingOptions.None);

                    if (valueType == typeof(Uri) || valueType == typeof(object))
                        return Convert.ToString(value, CultureInfo.InvariantCulture);

                    break;
            }

            throw new InvalidOperationException(
                $"The type {valueType.Name} can not be converted to a string unambiguously");
        }

        public static TEnum ToEnum<TEnum>(string value)
        {
            Enum result;

            if (TryStringToEnum(typeof (TEnum), value, out result))
                return (TEnum) (object) result;

            return default(TEnum);
        }

        public static DateTimeOffset? ToDateTimeOffset(string value)
        {
            return !string.IsNullOrWhiteSpace(value) ? XmlConvert.ToDateTimeOffset(value) : (DateTimeOffset?) null;
        }

        public static string GetValueAsString(object baseObject, string path)
        {
            return ToString(GetValue(baseObject, path));
        }

        public static void SetValueFromString(object baseObject, string path, string value)
        {
            if (baseObject == null)
                throw new ArgumentNullException(nameof(baseObject));

            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            object targetObject;
            MemberInfo targetMember;

            ResolvePath(baseObject, path, out targetObject, out targetMember);

            Type valueType = GetMemberType(targetMember);

            bool valueIsNull = string.IsNullOrEmpty(value);

            if (valueIsNull && valueType.IsNullable())
            {
                SetValue(targetObject, targetMember, null);
            }
            else
            {
                if (valueType.IsNullable())
                    valueType = valueType.GetGenericArguments()[0];

                switch (Type.GetTypeCode(valueType))
                {
                    case TypeCode.Boolean:
                        SetValue(targetObject, targetMember, !valueIsNull && XmlConvert.ToBoolean(value));
                        break;

                    case TypeCode.Char:
                        SetValue(targetObject, targetMember, valueIsNull ? '\0' : XmlConvert.ToChar(value));
                        break;

                    case TypeCode.SByte:
                        SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToSByte(value));
                        break;

                    case TypeCode.Byte:
                        SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToByte(value));
                        break;

                    case TypeCode.Int16:
                        SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToInt16(value));
                        break;

                    case TypeCode.UInt16:
                        SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToUInt16(value));
                        break;

                    case TypeCode.Int32:
                        {
                            Enum enumValue;

                            if (valueType.IsEnum && TryStringToEnum(valueType, value, out enumValue))
                                SetValue(targetObject, targetMember, enumValue);
                            else
                                SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToInt32(value));
                        }
                        break;

                    case TypeCode.UInt32:
                        SetValue(targetObject, targetMember, valueIsNull ? 0 : XmlConvert.ToUInt32(value));
                        break;

                    case TypeCode.Int64:
                        SetValue(targetObject, targetMember, valueIsNull ? 0L : XmlConvert.ToInt16(value));
                        break;

                    case TypeCode.UInt64:
                        SetValue(targetObject, targetMember, valueIsNull ? 0L : XmlConvert.ToUInt64(value));
                        break;

                    case TypeCode.Single:
                        SetValue(targetObject, targetMember, valueIsNull ? 0.0 : XmlConvert.ToSingle(value));
                        break;

                    case TypeCode.Double:
                        SetValue(targetObject, targetMember, valueIsNull ? 0.0 : XmlConvert.ToDouble(value));
                        break;

                    case TypeCode.Decimal:
                        SetValue(targetObject, targetMember, valueIsNull ? new decimal(0) : XmlConvert.ToDecimal(value));
                        break;

                    case TypeCode.DateTime:
                        SetValue(targetObject, targetMember,
                                 valueIsNull
                                     ? new DateTime(0)
                                     : XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind));
                        break;

                    case TypeCode.String:
                        SetValue(targetObject, targetMember, value);
                        break;

                    default:
                        if (valueType == typeof(TimeSpan))
                            SetValue(targetObject, targetMember,
                                     valueIsNull ? new TimeSpan() : XmlConvert.ToTimeSpan(value));

                        else if (valueType == typeof(Guid))
                            SetValue(targetObject, targetMember, valueIsNull ? Guid.Empty : XmlConvert.ToGuid(value));

                        else if (valueType == typeof(DateTimeOffset))
                            SetValue(targetObject, targetMember,
                                     valueIsNull ? new DateTimeOffset() : XmlConvert.ToDateTimeOffset(value));

                        else if (valueType == typeof(byte[]))
                            SetValue(targetObject, targetMember,
                                     valueIsNull ? new byte[0] : Convert.FromBase64String(value));

                        else if (valueType == typeof(Uri))
                            SetValue(targetMember, targetMember,
                                     valueIsNull ? null : new Uri(value, UriKind.RelativeOrAbsolute));

                        else if (valueType == typeof(object))
                            SetValue(targetObject, targetMember, value);

                        else
                            throw new NotSupportedException($"The type {valueType.Name} is not supported");

                        break;
                }
            }
        }
    }
}
