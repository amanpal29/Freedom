using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class LessThanOrEqualToConstraint : BinaryConstraint
    {
        public LessThanOrEqualToConstraint ()
        {
        }

        public LessThanOrEqualToConstraint([NotNull] string fieldName, object value)
            : base(fieldName, value)
        {
        }

        public override ConstraintType ConstraintType => ConstraintType.LessThanOrEqualTo;
    }
}