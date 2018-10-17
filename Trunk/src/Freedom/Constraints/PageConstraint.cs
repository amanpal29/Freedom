using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Namespace)]
    public class PageConstraint : Constraint
    {
        #region Fields

        private int _skip;
        private int _take = int.MaxValue;
        private IList<OrderByExpression> _orderBy = new List<OrderByExpression>();

        #endregion

        #region Properties

        [DataMember]
        public int Skip
        {
            get { return _skip; }
            set { _skip = value; }
        }

        [DataMember]
        public int Take
        {
            get { return _take; }
            set { _take = value; }
        }

        [DataMember]
        public IList<OrderByExpression> OrderBy
        {
            get { return _orderBy; }
            set { _orderBy = new List<OrderByExpression>(value); }
        }

        [DataMember]
        public Constraint InnerConstraint { get; set; }

        public bool IsAllRecords => _skip == 0 && _take == int.MaxValue;

        public void AddOrderBy(string fieldName, bool descending)
        {
            AddOrderBy(new OrderByExpression(fieldName, descending));
        }

        public void AddOrderBy(OrderByExpression orderByExpression)
        {
            _orderBy.Insert(0, orderByExpression);
        }

        #endregion

        #region Overrides of Constraint

        public override ConstraintType ConstraintType => ConstraintType.Page;

        public override bool IsEmpty => _skip == 0 && _take == int.MaxValue && (InnerConstraint == null || InnerConstraint.IsEmpty);

        public override bool IsValid
        {
            get
            {
                if (_skip < 0 || _take < 0)
                    return false;

                return InnerConstraint == null || InnerConstraint.IsValid;
            }
        }

        public override bool? ReducedValue
        {
            get
            {
                if (_take <= 0)
                    return false;

                return null;
            }
        }

        #endregion
    }
}
