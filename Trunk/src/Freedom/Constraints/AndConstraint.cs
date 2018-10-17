using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [CollectionDataContract(Namespace = Namespace)]
    public class AndConstraint : CompositeConstraint
    {
        #region Constructors

        public AndConstraint()
        {
        }

        public AndConstraint(params Constraint[] constraints)
        {
            AddRange(constraints);
        }

        public AndConstraint(IEnumerable<Constraint> constraints)
        {
            AddRange(constraints);
        }

        #endregion

        #region Overrides of Constraint

        public override ConstraintType ConstraintType => ConstraintType.And;

        public override bool? ReducedValue
        {
            get
            {
                if (this.All(c => c.ReducedValue == true))
                    return true;

                if (this.Any(c => c.ReducedValue == false))
                    return false;

                return null;
            }
        }
        #endregion
    }
}