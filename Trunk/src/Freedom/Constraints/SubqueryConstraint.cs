using System;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    /*
     * Represents a constraint based on a subquery:
     * eg:
     *      SELECT ...
     *      WHERE city IN (SELECT cityName FROM Cities WHERE province == "Alberta")
     *    
     *  would be:
     * 
     *  new SubqueryConstraint("city", "cityName", "Cities", new EqualConstraint("province", "Alberta"));
     * 
     */
    [DataContract(Namespace = Namespace)]
    public class SubqueryConstraint : Constraint
    {
        #region Constructor

        public SubqueryConstraint()
        {
        }

        public SubqueryConstraint(string outerPath, string innerPath, Type innerEntityType, Constraint innerConstraint)
        {
            OuterPath = outerPath;
            InnerPath = innerPath;
            InnerEntityTypeName = innerEntityType.Name;
            InnerConstraint = innerConstraint;
        }

        public SubqueryConstraint(string outerPath, string innerPath, string innerEntityTypeName, Constraint innerConstraint)
        {
            OuterPath = outerPath;
            InnerPath = innerPath;
            InnerEntityTypeName = innerEntityTypeName;
            InnerConstraint = innerConstraint;
        }

        #endregion

        #region Properties

        [DataMember]
        public string OuterPath { get; set; }

        [DataMember]
        public string InnerPath { get; set; }

        [DataMember]
        public string InnerEntityTypeName { get; set; }

        [DataMember]
        public Constraint InnerConstraint { get; set; }

        #endregion

        #region Overrides of Constraint

        public override ConstraintType ConstraintType => ConstraintType.Subquery;

        public override bool IsEmpty => string.IsNullOrEmpty(InnerEntityTypeName);

        public override bool IsValid => !string.IsNullOrWhiteSpace(OuterPath) &&
                                        !string.IsNullOrWhiteSpace(InnerPath) &&
                                        !string.IsNullOrWhiteSpace(InnerEntityTypeName);

        public override bool? ReducedValue
        {
            get
            {
                if (InnerConstraint != null && InnerConstraint.ReducedValue == false)
                    return false;

                return null;
            }
        }

        #endregion
    }
}
