using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Freedom.Constraints
{
    public class LinqPredicateConstraintVisitor
    {
        private static readonly MethodInfo ToStringMethod = typeof (object).GetMethod("ToString", new Type[0]);
        private static readonly MethodInfo StartsWithMethod = typeof (string).GetMethod("StartsWith", new [] { typeof(string) });
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        public static Expression<Func<T, bool>> BuildExpression<T>(Constraint constraint)
        {
            LinqPredicateConstraintVisitor visitor = new LinqPredicateConstraintVisitor();

            ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

            visitor._parameterStack.Push(parameter);

            return Expression.Lambda<Func<T, bool>>(visitor.Visit(constraint), parameter);
        }

        public static Func<T, bool> CompilePredicate<T>(Constraint constraint)
        {
            return BuildExpression<T>(constraint).Compile();
        }

        private readonly Stack<Expression> _parameterStack = new Stack<Expression>();

        private LinqPredicateConstraintVisitor()
        {
        }

        private Expression Visit(Constraint constraint)
        {
            if (constraint == null)
                return Expression.Constant(true);

            switch (constraint.ConstraintType)
            {
                case ConstraintType.Equal:
                    return VisitEqualConstraint((EqualConstraint)constraint);

                case ConstraintType.NotEqual:
                    return VisitNotEqualConstraint((NotEqualConstraint)constraint);

                case ConstraintType.GreaterThan:
                    return VisitGreaterThanConstraint((GreaterThanConstraint)constraint);

                case ConstraintType.LessThan:
                    return VisitLessThanConstraint((LessThanConstraint)constraint);

                case ConstraintType.GreaterThanOrEqualTo:
                    return VisitGreaterThanOrEqualToConstraint((GreaterThanOrEqualToConstraint)constraint);

                case ConstraintType.LessThanOrEqualTo:
                    return VisitLessThanOrEqualToConstraint((LessThanOrEqualToConstraint)constraint);

                case ConstraintType.And:
                    return VisitAndConstraint((AndConstraint)constraint);

                case ConstraintType.Or:
                    return VisitOrConstraint((OrConstraint)constraint);

                case ConstraintType.StringContains:
                    return VisitStringContainsConstraint((StringContainsConstraint)constraint);

                case ConstraintType.StartsWith:
                    return VisitStartsWithConstraint((StartsWithConstraint)constraint);

                default:
                    throw new ArgumentException($"Constraints of type {constraint.ConstraintType} are not supported.");
            }
        }


        private Expression Parameter => _parameterStack.Peek();

        private static PropertyInfo FindMember(Type type, string memberName)
        {
            PropertyInfo property = type.GetProperty(memberName,
                BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            if (property != null)
                return property;

            throw new InvalidOperationException("Property was not found");
        }

        private static Expression BuildConstantExpression(object value, Type target)
        {
            if (value == null || target.IsInstanceOfType(value))
                return Expression.Constant(value, target);

            return Expression.Convert(Expression.Constant(value), target);
        }

        public Expression VisitEqualConstraint(EqualConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.Equal(memberExpression, valueExpression);
        }

        public Expression VisitNotEqualConstraint(NotEqualConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.NotEqual(memberExpression, valueExpression);
        }

        public Expression VisitGreaterThanConstraint(GreaterThanConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.GreaterThan(memberExpression, valueExpression);
        }

        public Expression VisitLessThanConstraint(LessThanConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.LessThan(memberExpression, valueExpression);
        }

        public Expression VisitGreaterThanOrEqualToConstraint(GreaterThanOrEqualToConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.GreaterThanOrEqual(memberExpression, valueExpression);
        }

        public Expression VisitLessThanOrEqualToConstraint(LessThanOrEqualToConstraint constraint)
        {
            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            MemberExpression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            Expression valueExpression = BuildConstantExpression(constraint.Value, member.PropertyType);

            return Expression.LessThanOrEqual(memberExpression, valueExpression);
        }

        private Expression VisitOrConstraint(OrConstraint constraint)
        {
            if (constraint.IsEmpty)
                return Expression.Constant(false);

            if (constraint.Count == 1)
                return Visit(constraint[0]);

            int i = constraint.Count - 1;

            Expression expression = Visit(constraint[i]);

            for (i--; i >= 0; i--)
                expression = Expression.OrElse(Visit(constraint[i]), expression);

            return expression;
        }

        private Expression VisitAndConstraint(AndConstraint constraint)
        {
            if (constraint.IsEmpty)
                return Expression.Constant(true);

            if (constraint.Count == 1)
                return Visit(constraint[0]);

            int i = constraint.Count - 1;

            Expression expression = Visit(constraint[i]);

            for (i--; i >= 0; i--)
                expression = Expression.AndAlso(Visit(constraint[i]), expression);

            return expression;
        }

        private Expression VisitStringContainsConstraint(StringContainsConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Value))
                return Expression.Constant(true);

            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            Expression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            if (member.PropertyType != typeof(string))
            {
                memberExpression = Expression.Call(memberExpression, ToStringMethod);
            }

            Expression valueExpression = BuildConstantExpression(constraint.Value, typeof(string));

            return Expression.Call(memberExpression, ContainsMethod, valueExpression);
        }

        private Expression VisitStartsWithConstraint(StartsWithConstraint constraint)
        {
            if (string.IsNullOrEmpty(constraint.Value))
                return Expression.Constant(true);

            PropertyInfo member = FindMember(Parameter.Type, constraint.FieldName);

            Expression memberExpression = Expression.MakeMemberAccess(Parameter, member);

            if (member.PropertyType != typeof(string))
            {
                memberExpression = Expression.Call(memberExpression, ToStringMethod);
            }

            Expression valueExpression = BuildConstantExpression(constraint.Value, typeof(string));

            return Expression.Call(memberExpression, StartsWithMethod, valueExpression);
        }
    }
}
