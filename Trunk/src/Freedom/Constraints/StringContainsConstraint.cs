using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class StringContainsConstraint : Constraint
    {
        public StringContainsConstraint()
        {
        }

        public StringContainsConstraint(string fieldName, string value)
        {
            FieldName = fieldName;
            Value = value;
        }

        public override ConstraintType ConstraintType => ConstraintType.StringContains;

        public override bool IsEmpty => string.IsNullOrWhiteSpace(FieldName) || string.IsNullOrWhiteSpace(Value);

        public override bool IsValid => !string.IsNullOrWhiteSpace(FieldName) && Value != null;

        public override bool? ReducedValue
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                    return true;

                return null;
            }
        }
        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
