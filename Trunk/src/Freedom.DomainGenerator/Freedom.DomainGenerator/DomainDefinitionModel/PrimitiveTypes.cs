using System;
using System.Reflection;

namespace Freedom.DomainGenerator.DomainDefinitionModel
{
    public static class PrimitiveTypes
    {
        public static readonly PrimitiveType Binary = new PrimitiveType(
            "Binary", "varbinary(max)", "varbinary(max)", "byte[]", "xs:base64Binary");

        public static readonly PrimitiveType Boolean = new PrimitiveType(
            "Boolean", "bit", "bit", "bool", "xs:boolean");

        public static readonly PrimitiveType Currency = new PrimitiveType(
            "Decimal", "decimal", "decimal(19,4)", "decimal", "xs:decimal");

        public static readonly PrimitiveType Date = new PrimitiveType(
            "DateTime", "date", "date", "DateTime", "xs:date");

        public static readonly PrimitiveType DateTime = new PrimitiveType(
            "DateTimeOffset", "datetimeoffset", "datetimeoffset", "DateTimeOffset", "xs:dateTime");

        public static readonly PrimitiveType DateTimeStamp = new PrimitiveType(
            "DateTime", "datetime2", "datetime2", "DateTime", "xs:dateTime");

        public static readonly PrimitiveType Double = new PrimitiveType(
            "Double", "float", "float", "double", "xs:double");

        public static readonly PrimitiveType Guid = new PrimitiveType(
            "Guid", "uniqueidentifier", "uniqueidentifier", "Guid", "guid");

        public static readonly PrimitiveType Int32 = new PrimitiveType(
            "Int32", "int", "int", "int", "xs:integer");

        public static readonly PrimitiveType String = new PrimitiveType(
            "String", "nvarchar", "nvarchar(4000)", "string", "xs:string");

        public static readonly PrimitiveType Text = new PrimitiveType(
            "String", "nvarchar(max)", "nvarchar(max)", "string", "xs:string");

        public static readonly PrimitiveType Time = new PrimitiveType(
            "Time", "time", "time", "TimeSpan", "xs:time");

        public static readonly PrimitiveType ShortTimeSpan = new PrimitiveType(
            "Int32", "int", "int", "int", "xs:integer");

        public static PrimitiveType Get(string primitiveType)
        {
            if (string.IsNullOrEmpty(primitiveType))
                throw new ArgumentNullException(nameof(primitiveType));

            FieldInfo fieldInfo = typeof (PrimitiveTypes).GetField(primitiveType,
                BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);

            if (fieldInfo == null)
                throw new InvalidOperationException($"{primitiveType} is not a recognised primitive type");

            return (PrimitiveType) fieldInfo.GetValue(null);
        } 

    }
}
