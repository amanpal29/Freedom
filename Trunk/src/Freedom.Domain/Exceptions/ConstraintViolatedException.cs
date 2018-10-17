using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Domain.Exceptions
{
    public enum SqlConstraintType
    {
        Unknown,
        NotNull = 515,
        CheckOrForeignKey = 547,
        UniqueIndex = 2601,
        PrimaryKey = 2627
    }

    [Serializable]
    public class ConstraintViolatedException : Exception
    {
        public ConstraintViolatedException()
        {
        }

        public ConstraintViolatedException(string message)
            : base(message)
        {
        }

        public ConstraintViolatedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public ConstraintViolatedException(SqlConstraintType constraintType)
        {
            ConstraintType = constraintType;
        }

        public ConstraintViolatedException(SqlConstraintType constraintType, string message)
            : base(message)
        {
            ConstraintType = constraintType;
        }

        public ConstraintViolatedException(SqlConstraintType constraintType, string message, Exception inner)
            : base(message, inner)
        {
            ConstraintType = constraintType;
        }

        public ConstraintViolatedException(IDictionary<string, object> httpError, string message, Exception inner)
            : base(message, inner)
        {
            ConstraintType = GetConstraintType(httpError);

            if (httpError == null) return;

            Class = GetValue<byte?>(httpError, nameof(Class));
            LineNumber = GetValue<int?>(httpError, nameof(LineNumber));
            Number = GetValue<int?>(httpError, nameof(Number));
            Procedure = GetValue<string>(httpError, nameof(Procedure));
            Provider = GetValue<string>(httpError, nameof(Provider));
            Server = GetValue<string>(httpError, nameof(Server));
            State = GetValue<byte?>(httpError, nameof(State));
        }

        private static SqlConstraintType GetConstraintType(IDictionary<string, object> dictionary)
        {
            if (dictionary == null || !dictionary.ContainsKey(nameof(ConstraintType)))
                return SqlConstraintType.Unknown;

            object obj = dictionary[nameof(ConstraintType)];

            if (obj == null)
                return SqlConstraintType.Unknown;

            if (obj is int && Enum.IsDefined(typeof(SqlConstraintType), obj))
                return (SqlConstraintType)(int)obj;

            if (obj is long && Enum.IsDefined(typeof(SqlConstraintType), obj))
                return (SqlConstraintType)(int)(long)obj;

            SqlConstraintType constraintType;

            if (obj is string && Enum.TryParse((string)obj, true, out constraintType))
                return constraintType;

            return SqlConstraintType.Unknown;
        }

        private T GetValue<T>(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (!dictionary.ContainsKey(key))
                return default(T);

            object obj = dictionary[key];

            if (obj is T)
                return (T) obj;

            return default(T);
        }

        protected ConstraintViolatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ConstraintType = (SqlConstraintType) info.GetInt32(nameof(ConstraintType));
            Class = (byte?) info.GetValue(nameof(Class), typeof(byte?));
            LineNumber = (int?) info.GetValue(nameof(LineNumber), typeof(int?));
            Number = (int?) info.GetValue(nameof(Number), typeof(int?));
            Procedure = info.GetString(nameof(Procedure));
            Provider = info.GetString(nameof(Provider));
            Server = info.GetString(nameof(Server));
            State = (byte?) info.GetValue(nameof(State), typeof(byte?));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ConstraintType), (int) ConstraintType);
            info.AddValue(nameof(Class), Class);
            info.AddValue(nameof(LineNumber), LineNumber);
            info.AddValue(nameof(Number), Number);
            info.AddValue(nameof(Procedure), Procedure);
            info.AddValue(nameof(Provider), Provider);
            info.AddValue(nameof(Server), Server);
            info.AddValue(nameof(State), State);
        }

        public SqlConstraintType ConstraintType { get; }

        public byte? Class { get; set; }
        public int? LineNumber { get; set; }
        public int? Number { get; set; }
        public string Procedure { get; set; }
        public string Provider { get; set; }
        public string Server { get; set; }
        public byte? State { get; set; }
    }
}
