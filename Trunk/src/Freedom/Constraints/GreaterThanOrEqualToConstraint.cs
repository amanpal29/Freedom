using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class GreaterThanOrEqualToConstraint : BinaryConstraint
    {
        public GreaterThanOrEqualToConstraint()
        {
        }

        public GreaterThanOrEqualToConstraint([NotNull] string fieldName, object value)
            : base(fieldName, value)
        {
        }

        public override ConstraintType ConstraintType => ConstraintType.GreaterThanOrEqualTo;
    }
}