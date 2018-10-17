using System;
using System.Linq.Expressions;
using Freedom.Constraints;
using Freedom.Linq;

namespace Freedom.ViewModels.Filters
{
    public class FilterOptionViewModel : ViewModelBase
    {
        private Constraint _constraint;

        protected FilterOptionViewModel()
        {
        }

        protected FilterOptionViewModel(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public FilterOptionViewModel(string name, string description, Constraint constraint)
        {
            Name = name;
            Description = description;
            _constraint = constraint;
        }

        public string Name { get; }

        public string Description { get; }

        public virtual Constraint Constraint => _constraint;

        protected void SetConstraint(Constraint constraint) => _constraint = constraint;
    }

    public class FilterOptionViewModel<T> : FilterOptionViewModel
    {
        public FilterOptionViewModel(string name, string description, Expression<Func<T, bool>> predicate)
            : base(name, description)
        {
            Predicate = predicate?.Compile() ?? (x => true);
            Expression = predicate;
        }

        public Func<T, bool> Predicate { get; }

        public Expression<Func<T, bool>> Expression { get; }

        public override Constraint Constraint
        {
            get
            {
                if (base.Constraint == null && Expression != null)
                    SetConstraint(ConstraintBuilder.Build(Expression));

                return base.Constraint;
            }
        }
    }
}