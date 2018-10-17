using System;
using System.Collections.Generic;
using Freedom.Annotations;
using Freedom.ViewModels;

namespace Freedom.Extensions
{
    public static class ViewModelBaseExtensions
    {
        public static bool HasDescendant(this ViewModelBase root, [NotNull] ViewModelBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (root == item)
                return true;

            IEnumerable<ViewModelBase> children = root.Children;

            if (children == null)
                return false;

            foreach (ViewModelBase child in children)
                if (child != null && child.HasDescendant(item))
                    return true;

            return false;
        }

        public static IEnumerable<T> GetDescendants<T>(this ViewModelBase root)
        {
            HashSet<ViewModelBase> visited = new HashSet<ViewModelBase>();
            Queue<ViewModelBase> queue = new Queue<ViewModelBase>();

            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                ViewModelBase current = queue.Dequeue();

                if (!visited.Add(current)) continue;

                if (current is T)
                    yield return (T) (object) current;

                IEnumerable<ViewModelBase> children = current.Children;

                if (children == null) continue;

                foreach (ViewModelBase child in children)
                    if (child != null)
                        queue.Enqueue(child);
            }
        }
    }
}
