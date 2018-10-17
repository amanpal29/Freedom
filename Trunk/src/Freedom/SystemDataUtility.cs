using System;
using System.Data;

namespace Freedom
{
    public static class SystemDataUtility
    {
        public static DbType GetDbTypeFromSqlStorageType(string storageType)
        {
            int length = storageType.IndexOf("(", StringComparison.Ordinal);

            string baseType = storageType
                .Substring(0, length < 0 ? storageType.Length : length)
                .ToLowerInvariant();

            switch (baseType)
            {
                case "bigint":
                    return DbType.Int64;

                case "bit":
                    return DbType.Boolean;

                case "char":
                    return DbType.AnsiStringFixedLength;

                case "date":
                    return DbType.Date;

                case "datetime":
                case "smalldatetime":
                    return DbType.DateTime;

                case "datetime2":
                    return DbType.DateTime2;

                case "datetimeoffset":
                    return DbType.DateTimeOffset;

                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return DbType.Decimal;

                case "float":
                    return DbType.Double;

                case "int":
                    return DbType.Int32;

                case "nchar":
                    return DbType.StringFixedLength;

                case "nvarchar":
                case "ntext":
                    return DbType.String;

                case "real":
                    return DbType.Single;

                case "smallint":
                    return DbType.Int16;

                case "time":
                    return DbType.Time;

                case "tinyint":
                    return DbType.Byte;

                case "uniqueidentifier":
                    return DbType.Guid;

                case "binary":
                case "varbinary":
                case "image":
                    return DbType.Binary;

                case "varchar":
                case "text":
                    return DbType.AnsiString;

                case "xml":
                    return DbType.Xml;

                default:
                    throw new InvalidOperationException($"Unsupported storage type '{storageType}'.");
            }
        }

    }
}
