using System;
using System.Collections.Generic;

namespace Freedom.Constraints
{
    public static class ConstraintHelper
    {
        public static TConstraint FindConstraint<TConstraint>(this Constraint root, Predicate<TConstraint> predicate)
            where TConstraint : Constraint
        {
            Queue<Constraint> queue = new Queue<Constraint>();

            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Constraint current = queue.Dequeue();

                if (current is TConstraint && predicate((TConstraint) current))
                    return (TConstraint) current;

                if (current is IEnumerable<Constraint>)
                {
                    foreach (Constraint child in (IEnumerable<Constraint>) current)
                        queue.Enqueue(child);
                }
                else if (current is SubqueryConstraint)
                {
                    queue.Enqueue(((SubqueryConstraint) current).InnerConstraint);
                }
                else if (current is PageConstraint)
                {
                    queue.Enqueue(((PageConstraint) current).InnerConstraint);
                }
            }

            return null;
        }
    }
}
