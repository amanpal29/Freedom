using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Xml;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    [KnownType(typeof(EqualConstraint))]
    [KnownType(typeof(NotEqualConstraint))]
    [KnownType(typeof(LessThanConstraint))]
    [KnownType(typeof(LessThanOrEqualToConstraint))]
    [KnownType(typeof(GreaterThanConstraint))]
    [KnownType(typeof(GreaterThanOrEqualToConstraint))]
    public abstract class BinaryConstraint : Constraint
    {
        private const string NodeTypeNotSupported = @"Expressions of type {0} are not supported";

        private object _value;

        protected BinaryConstraint()
        {
        }

        protected BinaryConstraint([NotNull] string fieldName, object value)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            if (!IsSupportedValue(value))
                throw new ArgumentException(value.GetType().FullName + " is not a supported type for the value parameter.", nameof(value));

            FieldName = fieldName;
            Value = value;
        }

        public static BinaryConstraint Build(ExpressionType expressionType, string path, object value)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return new EqualConstraint(path, value);

                case ExpressionType.NotEqual:
                    return new NotEqualConstraint(path, value);

                case ExpressionType.GreaterThan:
                    return new GreaterThanConstraint(path, value);

                case ExpressionType.GreaterThanOrEqual:
                    return new GreaterThanOrEqualToConstraint(path, value);

                case ExpressionType.LessThan:
                    return new LessThanConstraint(path, value);

                case ExpressionType.LessThanOrEqual:
                    return new LessThanOrEqualToConstraint(path, value);
            }

            throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expressionType));
        }

        public static ExpressionType FlipBinaryExpressionType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.Equal:
                    return ExpressionType.Equal;

                case ExpressionType.NotEqual:
                    return ExpressionType.NotEqual;

                case ExpressionType.GreaterThan:
                    return ExpressionType.LessThan;

                case ExpressionType.GreaterThanOrEqual:
                    return ExpressionType.LessThanOrEqual;

                case ExpressionType.LessThan:
                    return ExpressionType.GreaterThan;

                case ExpressionType.LessThanOrEqual:
                    return ExpressionType.GreaterThanOrEqual;
            }

            throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expressionType));
        }

        public override bool IsEmpty => string.IsNullOrWhiteSpace(FieldName);

        public override bool IsValid => !string.IsNullOrWhiteSpace(FieldName);

        [DataMember]
        public string FieldName { get; set; }

        public object Value
        {
            get { return _value; }
            set
            {
                if (value == null)
                {
                    _value = null;
                    return;
                }

                if (value.GetType().IsEnum)
                {
                    _value = (int) value;
                    return;
                }

                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Empty:
                    case TypeCode.DBNull:
                        _value = null;
                        break;

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                        _value = (int) value;
                        break;

                    case TypeCode.Byte:
                    case TypeCode.UInt16:
                        _value = (uint) value;
                        break;

                    case TypeCode.Single:
                        _value = (double) value;
                        break;

                    case TypeCode.Object:
                        if (value is DateTimeOffset || value is Guid)
                            goto default;

                        throw new ArgumentException(
                            "The type " + value.GetType().FullName +
                            " is not supported as a literal value in a binary constraint.", nameof(value));

                    default:
                        _value = value;
                        break;
                }
            }
        }

        #region Static Helper Methods

        public static bool IsSupportedValue(object value)
        {
            return value == null || Type.GetTypeCode(value.GetType()) != TypeCode.Object || value is Guid || value is DateTimeOffset;
        }

        #endregion

        #region TypeSpecificValueProperties

        // These properties are only used during serialization.
        // Only one of the following properties will be serialized, depending on the type of the Value
        // This avoids some ugly attributes in the XML to specify type when serializing constraints.
        // DGG - 2015-04-22

        [DataMember(EmitDefaultValue = false, Name = "IsNil")]
        protected bool ValueIsNull
        {
            get { return _value == null; }
            set
            {
                if (!value)
                    throw new InvalidOperationException("ValueIsNull can not be explicitally set to false.");

                 _value = null;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "DateTimeOffsetValue")]
        protected string ValueAsDateTimeOffset
        {
            get
            {
                if (!(_value is DateTimeOffset))
                    return null;

                return XmlConvert.ToString((DateTimeOffset) _value);
            }
            set { _value = XmlConvert.ToDateTimeOffset(value); }
        }

        [DataMember(EmitDefaultValue = false, Name = "GuidValue")]
        protected Guid? ValueAsGuid
        {
            get { return Value as Guid?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "BooleanValue")]
        protected bool? ValueAsBool
        {
            get { return Value as bool?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "CharValue")]
        protected char? ValueAsChar
        {
            get { return Value as char?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Int32Value")]
        protected int? ValueAsInt
        {
            get { return Value as int?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "UInt32Value")]
        protected uint? ValueAsUInt
        {
            get { return Value as uint?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "Int64Value")]
        protected long? ValueAsLong
        {
            get { return Value as long?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "UInt64Value")]
        protected ulong? ValueAsULong
        {
            get { return Value as ulong?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "DoubleValue")]
        protected double? ValueAsDouble
        {
            get { return Value as double?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "DecimalValue")]
        protected decimal? ValueAsDecimal
        {
            get { return Value as decimal?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "DateTimeValue")]
        protected DateTime? ValueAsDateTime
        {
            get { return Value as DateTime?; }
            set { Value = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "StringValue")]
        protected string ValueAsString
        {
            get { return Value as string; }
            set { Value = value; }
        }

        #endregion
    }
}
