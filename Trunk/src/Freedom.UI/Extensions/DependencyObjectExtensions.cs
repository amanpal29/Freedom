using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Freedom.UI.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T FindVisualAncestor<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            DependencyObject currentObject = obj;

            while (currentObject != null && !(currentObject is T))
                currentObject = VisualTreeHelper.GetParent(currentObject);

            return (T)currentObject;
        }

        public static T FindVisualDescendant<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            return FindVisualDescendant<T>(obj, x => true);
        }

        public static T FindVisualDescendant<T>(this DependencyObject obj, Predicate<T> predicate)
            where T : DependencyObject
        {
            Queue<DependencyObject> queue = new Queue<DependencyObject>();

            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                DependencyObject currentObject = queue.Dequeue();

                T element = currentObject as T;

                if (element != null && predicate(element))
                    return element;

                int childCount = VisualTreeHelper.GetChildrenCount(currentObject);

                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(currentObject, i);

                    if (child != null)
                        queue.Enqueue(child);
                }
            }

            return null;
        }

        public static IEnumerable<DependencyObject> GetDescendants<T>(this DependencyObject obj, Predicate<T> predicate)
            where T : DependencyObject
        {
            Queue<DependencyObject> queue = new Queue<DependencyObject>();

            queue.Enqueue(obj);

            while (queue.Count > 0)
            {
                DependencyObject currentObject = queue.Dequeue();

                T element = currentObject as T;

                if (element != null && predicate(element))
                    yield return element;

                int childCount = VisualTreeHelper.GetChildrenCount(currentObject);

                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(currentObject, i);

                    if (child != null)
                        queue.Enqueue(child);
                }
            }
        }

        public static TControl FindFocusedControl<TControl>(this DependencyObject obj)
           where TControl : class
        {
            DependencyObject dependencyObject = obj;

            while (dependencyObject != null)
            {
                if (FocusManager.GetIsFocusScope(dependencyObject))
                {
                    IInputElement currentFocus = FocusManager.GetFocusedElement(dependencyObject);

                    if (currentFocus is TControl)
                        return (TControl)currentFocus;
                }

                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return null;
        }
    }
}
