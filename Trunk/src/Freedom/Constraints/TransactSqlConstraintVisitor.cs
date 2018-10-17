using System;

namespace Freedom.Constraints
{
    public class TransactSqlConstraintVisitor : SqlConstraintVisitor
    {
        public TransactSqlConstraintVisitor()
        {
        }

        public TransactSqlConstraintVisitor(string tableSource)
            : base(tableSource)
        {
        }

        protected override string EscapeIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentNullException(nameof(identifier));

            if (identifier.Length > 128)
                throw new ArgumentException("identifier can't be longer than 128 characters", nameof(identifier));

            return "[" + identifier.Replace("]", "]]") + "]";
        }
    }
}
