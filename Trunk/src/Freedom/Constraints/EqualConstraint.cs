using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class EqualConstraint : BinaryConstraint
    {
        public EqualConstraint()
        {
        }

        public EqualConstraint([NotNull] string fieldName, object value) : base(fieldName, value)
        {
        }

        public override ConstraintType ConstraintType => ConstraintType.Equal;
    }
}