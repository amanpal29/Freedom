using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class InKeySetConstraint : Constraint
    {
        private List<Guid> _values;

        public InKeySetConstraint()
        {
        }

        public InKeySetConstraint(string fieldName, params Guid[] values)
            : this(fieldName, (IEnumerable<Guid>) values)
        {
        }

        public InKeySetConstraint(string fieldName, IEnumerable<Guid> values)
        {
            FieldName = fieldName;
            _values = new List<Guid>(values);
        }

        public override ConstraintType ConstraintType => ConstraintType.InKeySet;

        public override bool IsEmpty => string.IsNullOrWhiteSpace(FieldName);

        public override bool IsValid => !string.IsNullOrWhiteSpace(FieldName);

        public override bool? ReducedValue
        {
            get
            {
                if (_values.Count == 0)
                    return false;

                return null;
            }
        }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public IList<Guid> Values => _values ?? (_values = new List<Guid>());
    }
}
