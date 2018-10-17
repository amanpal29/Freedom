using System;
using System.Linq.Expressions;
using Freedom.Domain.Model;
using Freedom.ComponentModel;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Query
{
    public static class ConfidentialEntityHelper
    {
        public static Expression<Func<T, bool>> GetRequestedExpression<T>(Constraint constraint, FreedomLocalContext context)
        {
            Constraint innerConstraint = GetInnerConstraint(typeof (T), constraint);

            DbContextConstraintVisitor visitor = new DbContextConstraintVisitor(context);

            return visitor.BuildLambdaPredicateExpression<T>(innerConstraint);
        }

        public static Constraint GetInnerConstraint(Type entityType, Constraint constraint)
        {
            if (entityType == null)
                throw new ArgumentNullException(nameof(entityType));

            if (!ConfidentialAttribute.IsDefined(entityType))
                throw new ArgumentException($"The entityType {entityType.FullName} is not a confidential entity type.", nameof(entityType));

            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));

            PageConstraint pageConstraint = constraint as PageConstraint;

            if (pageConstraint != null)
            {
                if (pageConstraint.Skip != 0 || (pageConstraint.Take != 1 && pageConstraint.Take != 2))
                    throw new ArgumentException(
                        "Page constraints requesting mutiple entities are not permitted when accessing confidential entities.",
                        nameof(constraint));

                constraint = pageConstraint.InnerConstraint;
            }

            return constraint;
        }
    }
}
