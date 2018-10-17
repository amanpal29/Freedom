using System;
using Freedom.Constraints;

namespace Freedom.ViewModels.Filters
{
    public class SeparatorFilterOptionViewModel : FilterOptionViewModel
    {
        public override Constraint Constraint
        {
            get { throw new InvalidOperationException(); }
        }
    }
}
