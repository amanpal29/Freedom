using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using Freedom.Extensions;

namespace Freedom.Constraints
{
    public abstract class SqlConstraintVisitor
    {
        private static readonly char[] WildcardCharsToEscape = { '[', ']', '%', '_', '\\' };

        protected SqlConstraintVisitor()
        {
            SourceTypes = new Dictionary<string, Type>();
            ParameterValues = new Dictionary<string, object>();
        }

        protected SqlConstraintVisitor(string tableSource)
            : this()
        {
            RootTableSource = tableSource;
        }

        public IDictionary<string, Type> SourceTypes { get; set; }

        public Dictionary<string, object> ParameterValues { get; }

        protected string RootTableSource { get; }

        protected Stack<string> TableSourceStack { get; } = new Stack<string>();

        protected string Alias => TableSourceStack.Count > 0 ? $"s{TableSourceStack.Count}" : "it";

        protected string TableSource => TableSourceStack.Count > 0 ? TableSourceStack.Peek() : RootTableSource;

        protected StringBuilder Sql { get; } = new StringBuilder();

        public virtual string SqlText => Sql.ToString();

        public virtual void Visit(Constraint constraint)
        {
            if (constraint == null)
                throw new ArgumentNullException(nameof(constraint));

            if (!constraint.IsValid)
                throw new ArgumentException("The constraint is not valid.", nameof(constraint));

            switch (constraint.ConstraintType)
            {
                case ConstraintType.Equal:
                    VisitEqualConstraint((EqualConstraint)constraint);
                    break;

                case ConstraintType.NotEqual:
                    VisitNotEqualConstraint((NotEqualConstraint)constraint);
                    break;

                case ConstraintType.GreaterThanOrEqualTo:
                    VisitGreaterThanOrEqualToConstraint((GreaterThanOrEqualToConstraint)constraint);
                    break;

                case ConstraintType.LessThanOrEqualTo:
                    VisitLessThanOrEqualToConstraint((LessThanOrEqualToConstraint)constraint);
                    break;

                case ConstraintType.GreaterThan:
                    VisitGreaterThanConstraint((GreaterThanConstraint) constraint);
                    break;

                case ConstraintType.LessThan:
                    VisitLessThanConstraint((LessThanConstraint)constraint);
                    break;

                case ConstraintType.And:
                    VisitAndConstraint((AndConstraint)constraint);
                    break;

                case ConstraintType.Or:
                    VisitOrConstraint((OrConstraint)constraint);
                    break;

                case ConstraintType.Subquery:
                    VisitSubqueryConstraint((SubqueryConstraint) constraint);
                    break;

                case ConstraintType.InKeySet:
                    VisitInKeySetConstraint((InKeySetConstraint) constraint);
                    break;

                case ConstraintType.StartsWith:
                    VisitStartsWithConstraint((StartsWithConstraint)constraint);
                    break;

                case ConstraintType.StringContains:
                    VisitStringContainsConstraint((StringContainsConstraint) constraint);
                    break;

                case ConstraintType.FullTextSearch:
                    VisitFullTextSearchConstraint((FullTextSearchConstraint)constraint);
                    break;

                case ConstraintType.Page:
                    VisitPageConstraint((PageConstraint) constraint);
                    break;

                default:
                    throw new ArgumentException($"Constraints of type {constraint.ConstraintType} are not supported.", nameof(constraint));
            }
        }

        protected string AddParameter(object parameterValue)
        {
            string parameterName = $"p{ParameterValues.Count}";

            if (parameterValue is DateTimeOffset)
            {
                DateTimeOffset dateTimeOffset = (DateTimeOffset) parameterValue;

                parameterValue = dateTimeOffset.ToUniversalTime().DateTime;
            }

            ParameterValues.Add(parameterName, parameterValue);

            return parameterName;
        }

        protected abstract string EscapeIdentifier(string identifier);

        protected virtual string EscapeLikeWildcards(string input)
        {
            if (string.IsNullOrEmpty(input) || input.IndexOfAny(WildcardCharsToEscape) < 0)
                return input;

            StringBuilder output = new StringBuilder(input);

            for (int i = 0; i < output.Length; i++)
                if (WildcardCharsToEscape.Contains(output[i]))
                    output.Insert(i++, '\\');

            return output.ToString();
        }

        protected virtual string GetColumnIdentifier(string fieldName)
        {
            if (string.IsNullOrEmpty(TableSource))
                return fieldName;

            if (!SourceTypes.ContainsKey(TableSource))
                return fieldName;

            Type entityType = SourceTypes[TableSource];

            if (entityType == null)
                return fieldName;

            PropertyInfo property = entityType.GetProperty(fieldName);

            if (property == null)
                return fieldName;

            ColumnAttribute columnAttribute = property.GetAttribute<ColumnAttribute>();

            return columnAttribute?.Name ?? fieldName;
        }

        protected virtual void VisitEqualConstraint(EqualConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            if (constraint.Value == null)
            {
                Sql.AppendFormat("{0}.{1} IS NULL", Alias, columnIdentifier);
            }
            else
            {
                string parameterName = AddParameter(constraint.Value);

                Sql.AppendFormat("{0}.{1} = @{2}", Alias, columnIdentifier, parameterName);  
            }
        }

        protected virtual void VisitNotEqualConstraint(NotEqualConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            if (constraint.Value == null)
            {
                Sql.AppendFormat("{0}.{1} IS NOT NULL", Alias, columnIdentifier);
            }
            else
            {
                string parameterName = AddParameter(constraint.Value);

                Sql.AppendFormat("{0}.{1} != @{2}", Alias, columnIdentifier, parameterName);  
            }
        }
        protected virtual void VisitGreaterThanOrEqualToConstraint(GreaterThanOrEqualToConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            string parameterName = AddParameter(constraint.Value);

            Sql.AppendFormat("{0}.{1} >= @{2}", Alias, columnIdentifier, parameterName);
        }

        protected virtual void VisitLessThanOrEqualToConstraint(LessThanOrEqualToConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            string parameterName = AddParameter(constraint.Value);

            Sql.AppendFormat("{0}.{1} <= @{2}", Alias, columnIdentifier, parameterName);
        }

        protected virtual void VisitGreaterThanConstraint(GreaterThanConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            string parameterName = AddParameter(constraint.Value);

            Sql.AppendFormat("{0}.{1} > @{2}", Alias, columnIdentifier, parameterName);
        }

        protected virtual void VisitLessThanConstraint(LessThanConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            string parameterName = AddParameter(constraint.Value);

            Sql.AppendFormat("{0}.{1} < @{2}", Alias, columnIdentifier, parameterName);
        }

        protected virtual void VisitAndConstraint(IEnumerable<Constraint> constraint)
        {
            bool first = true;

            foreach (Constraint child in constraint)
            {
                if (child.IsEmpty) continue;

                if (first)
                    first = false;
                else
                    Sql.Append(" AND ");

                Sql.Append("(");

                Visit(child);

                Sql.Append(")");
            }
        }

        protected virtual void VisitOrConstraint(IEnumerable<Constraint> constraint)
        {
            bool first = true;

            foreach (Constraint child in constraint)
            {
                if (child.IsEmpty) continue;

                if (first)
                    first = false;
                else
                    Sql.Append(" OR ");

                Sql.Append("(");

                Visit(child);

                Sql.Append(")");
            }
        }

        protected virtual void VisitSubqueryConstraint(SubqueryConstraint constraint)
        {
            string qualifiedOuterPath = string.Join(".", Alias, EscapeIdentifier(constraint.OuterPath));

            Sql.AppendFormat("{0} IN (", qualifiedOuterPath);

            TableSourceStack.Push(constraint.InnerEntityTypeName);

            string qualifiedInnerPath = string.Join(".", Alias, EscapeIdentifier(constraint.InnerPath));

            Sql.AppendFormat("SELECT {0} FROM {1} AS {2}", qualifiedInnerPath, EscapeIdentifier(constraint.InnerEntityTypeName), Alias);

            if (constraint.InnerConstraint != null && !constraint.InnerConstraint.IsEmpty)
            {
                Sql.Append(" WHERE ");

                Visit(constraint.InnerConstraint);
            }

            TableSourceStack.Pop();

            Sql.Append(')');
        }

        protected virtual void VisitInKeySetConstraint(InKeySetConstraint constraint)
        {
            if (constraint.Values.Count == 0)
                throw new InvalidOperationException("InKeySetConstraint can't containt an empty set.");

            Sql.AppendFormat("{0}.{1} IN (", Alias, EscapeIdentifier(constraint.FieldName));

            foreach (Guid value in constraint.Values)
                Sql.AppendFormat("'{0}',", value);

            Sql.Length--;

            Sql.Append(")");
        }

        protected virtual void VisitStringContainsConstraint(StringContainsConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            if (constraint.Value == string.Empty)
            {
                Sql.AppendFormat("{0}.{1} IS NOT NULL", Alias, columnIdentifier);
            }
            else
            {
                string parameterName = AddParameter("%" + EscapeLikeWildcards(constraint.Value) + "%");

                Sql.AppendFormat("{0}.{1} LIKE @{2} ESCAPE '\\'", Alias, columnIdentifier, parameterName);
            }
        }

        protected virtual void VisitStartsWithConstraint(StartsWithConstraint constraint)
        {
            string columnIdentifier = EscapeIdentifier(GetColumnIdentifier(constraint.FieldName));

            if (constraint.Value == string.Empty)
            {
                Sql.AppendFormat("{0}.{1} IS NOT NULL", Alias, columnIdentifier);
            }
            else
            {
                string parameterName = AddParameter(EscapeLikeWildcards(constraint.Value) + "%");

                Sql.AppendFormat("{0}.{1} LIKE @{2} ESCAPE '\\'", Alias, columnIdentifier, parameterName);
            }
        }

        protected virtual void VisitFullTextSearchConstraint(FullTextSearchConstraint constraint)
        {
            throw new NotSupportedException("FullTextSearchConstraints are not supported.");
        }

        protected virtual void VisitPageConstraint(PageConstraint constraint)
        {
            Visit(constraint.InnerConstraint);
        }
    }
}