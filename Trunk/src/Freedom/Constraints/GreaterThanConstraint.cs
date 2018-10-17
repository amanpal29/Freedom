using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class GreaterThanConstraint : BinaryConstraint
    {
        public GreaterThanConstraint ()
        {
        }

        public GreaterThanConstraint([NotNull] string fieldName, object value)
            : base(fieldName, value)
        {
        }
        public override ConstraintType ConstraintType => ConstraintType.GreaterThan;
    }
}