using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class NotEqualConstraint : BinaryConstraint
    {
        public NotEqualConstraint ()
        {
        }

        public NotEqualConstraint([NotNull] string fieldName, object value)
            : base(fieldName, value)
        {
        }

        public override ConstraintType ConstraintType => ConstraintType.NotEqual;
    }
}