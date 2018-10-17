using System;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    [KnownType(typeof(BinaryConstraint))]
    [KnownType(typeof(CompositeConstraint))]
    [KnownType(typeof(SubqueryConstraint))]
    [KnownType(typeof(InKeySetConstraint))]
    [KnownType(typeof(StartsWithConstraint))]
    [KnownType(typeof(StringContainsConstraint))]
    [KnownType(typeof(FullTextSearchConstraint))]
    [KnownType(typeof(PageConstraint))]
    public abstract class Constraint
    {
        public const string Namespace = "http://schemas.Freedomsoftware.com";

        public abstract ConstraintType ConstraintType { get; }

        public abstract bool IsEmpty { get; }

        public abstract bool IsValid { get; }

        public virtual bool CanReduce => ReducedValue.HasValue;

        public virtual bool? ReducedValue => null;

        public virtual bool IsMatch<TObject>(TObject obj) { throw new NotImplementedException(); }
    }
}
