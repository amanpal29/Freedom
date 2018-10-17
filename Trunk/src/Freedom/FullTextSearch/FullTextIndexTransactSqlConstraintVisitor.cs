using System;
using Freedom.Constraints;

namespace Freedom.FullTextSearch
{
    public class FullTextIndexTransactSqlConstraintVisitor : TransactSqlConstraintVisitor
    {
        private readonly IWordBreaker _wordBreaker = IoC.TryGet<IWordBreaker>() ?? new StandardEnglishWordBreaker();

        public FullTextIndexTransactSqlConstraintVisitor(string tableSource)
            : base(tableSource)
        {
        }

        protected int SourceCode => TableSource.GetHashCode();

        protected override void VisitFullTextSearchConstraint(FullTextSearchConstraint constraint)
        {
            if (constraint == null || constraint.IsEmpty)
                throw new InvalidOperationException("Full Text Search Constraints can not be empty.");

            string[] searchTerms = _wordBreaker.BreakText(constraint.SearchText);

            string sourceParameter = AddParameter(SourceCode);

            Sql.Append('(');

            for (int i = 0; i < searchTerms.Length; i++)
            {
                if (i > 0)
                    Sql.Append(" AND ");

                string parameterName = AddParameter(searchTerms[i]);

                Sql.AppendFormat(
                    "({0}.[Id] IN (SELECT [Id] FROM [_FullTextIndex] WHERE [Source] = @{1} and [Keyword] = @{2}))",
                    Alias, sourceParameter, parameterName);
            }

            Sql.Append(')');
        }
    }
}