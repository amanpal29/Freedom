using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class LessThanConstraint : BinaryConstraint
    {
        public LessThanConstraint ()
        {
        }

        public LessThanConstraint([NotNull] string fieldName, object value)
            : base(fieldName, value)
        {
        }

        public override ConstraintType ConstraintType => ConstraintType.LessThan;
    }
}