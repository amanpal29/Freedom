using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [CollectionDataContract(Namespace = Namespace)]
    public class OrConstraint : CompositeConstraint
    {
        #region Constructors

        public OrConstraint()
        {
        }

        public OrConstraint(params Constraint[] constraints)
        {
            AddRange(constraints);
        }

        public OrConstraint(IEnumerable<Constraint> constraints)
        {
            AddRange(constraints);
        }

        #endregion

        #region Overrides of Constraint

        public override ConstraintType ConstraintType => ConstraintType.Or;

        public override bool? ReducedValue
        {
            get
            {
                if (this.All(c => c.ReducedValue == false))
                    return false;

                if (this.Any(c => c.ReducedValue == true))
                    return true;

                return null;
            }
        }

        #endregion
    }
}