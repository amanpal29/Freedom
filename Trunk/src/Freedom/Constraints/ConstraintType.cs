namespace Freedom.Constraints
{
    public enum ConstraintType
    {
        Invalid,

        // Binary constraints

        Equal,

        NotEqual,

        GreaterThanOrEqualTo,

        LessThanOrEqualTo,

        GreaterThan,

        LessThan,

        // Composite constraints

        And,

        Or,

        // Set constraints

        Subquery,

        InKeySet,

        // String matching constraints

        StartsWith,

        StringContains,

        // Full text searching constraints

        FullTextSearch,

        // Paging constraint

        Page
    }
}