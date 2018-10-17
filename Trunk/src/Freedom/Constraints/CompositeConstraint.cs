using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [CollectionDataContract(Namespace = Namespace)]
    [KnownType(typeof(AndConstraint))]
    [KnownType(typeof(OrConstraint))]
    public abstract class CompositeConstraint : Constraint, IList<Constraint>
    {
        #region Fields

        private const string NodeTypeNotSupported = @"Expressions of type {0} are not supported";

        private readonly List<Constraint> _constraints = new List<Constraint>();

        #endregion

        #region Abstract Factory Constructor

        public static CompositeConstraint Build(ExpressionType expressionType, params Constraint[] constraints)
        {
            CompositeConstraint result;

            switch (expressionType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    result = new AndConstraint();
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    result = new OrConstraint();
                    break;

                default:
                    throw new InvalidOperationException(string.Format(NodeTypeNotSupported, expressionType));
            }

            foreach (Constraint constraint in constraints)
            {
                if (result.GetType() == constraint.GetType())
                    result.AddRange((CompositeConstraint) constraint);
                else
                    result.Add(constraint);
            }

            return result;
        }

        #endregion

        #region Overrides of Constraint

        public override bool IsEmpty
        {
            get { return _constraints.TrueForAll(c => c.IsEmpty); }
        }

        public override bool IsValid
        {
            get { return _constraints.TrueForAll(c => c.IsValid); }
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<Constraint> GetEnumerator()
        {
            return _constraints.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<Constraint>

        public void Add(Constraint item)
        {
            if (item == null) return;

            if (item == this)
                throw new ArgumentException("A CompositeConstraint can not contain itself.", nameof(item));

            if (item.GetType() == GetType())
                AddRange((CompositeConstraint) item);
            else
                _constraints.Add(item);
        }

        public void AddRange(IEnumerable<Constraint> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items == this)
                throw new ArgumentException("A CompositeConstraint can not contain itself.", nameof(items));

            foreach (Constraint item in items)
                Add(item);
        }

        public void Clear()
        {
            _constraints.Clear();
        }

        public bool Contains(Constraint item)
        {
            return _constraints.Contains(item);
        }

        public void CopyTo(Constraint[] array, int arrayIndex)
        {
            _constraints.CopyTo(array, arrayIndex);
        }

        public bool Remove(Constraint item)
        {
            return _constraints.Remove(item);
        }

        public int Count => _constraints.Count;

        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<Constraint>

        public int IndexOf(Constraint item)
        {
            return _constraints.IndexOf(item);
        }

        public void Insert(int index, Constraint item)
        {
            _constraints.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _constraints.RemoveAt(index);
        }

        public Constraint this[int index]
        {
            get { return _constraints[index]; }
            set { _constraints[index] = value; }
        }

        #endregion
    }
}
