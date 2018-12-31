using Freedom.UI.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Freedom.UI
{
    public static class FocusHelper
    {
        private static readonly Dictionary<Type, DependencyProperty> ValueProperties = new Dictionary<Type, DependencyProperty>();

        public static void RegisterValueProperty(Type type, DependencyProperty dependencyProperty)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (dependencyProperty == null)
                throw new ArgumentNullException(nameof(dependencyProperty));

            if (!typeof(FrameworkElement).IsAssignableFrom(type))
                throw new ArgumentException("type must be a descandant of FrameworkElement", nameof(type));

            if (ValueProperties.ContainsKey(type))
                throw new InvalidOperationException($"There is already a value property registered for type {type}");

            ValueProperties.Add(type, dependencyProperty);
        }

        /// <summary>
        /// Fires the LoseFocus event on the control that currently has keyboard focus.
        /// </summary>
        public static void LoseCurrentFocus()
        {
            // Walk up the visual tree, if any of the controls have a value property registered, update it's binding

            int bindingsUpdated = 0;

            DependencyObject obj = Keyboard.FocusedElement as UIElement;

            while (obj != null)
            {
                Type objectType = obj.GetType();

                if (ValueProperties.ContainsKey(objectType))
                {
                    FrameworkElement frameworkElement = (FrameworkElement)obj;

                    DependencyProperty valueProprty = ValueProperties[objectType];

                    BindingExpression expression = frameworkElement.GetBindingExpression(valueProprty);

                    if (expression != null && expression.Status != BindingStatus.Detached)
                    {
                        bindingsUpdated++;
                        expression.UpdateSource();
                    }

                }

                obj = VisualTreeHelper.GetParent(obj);
            }

            if (bindingsUpdated != 0) return;

            UIElement uiElement = Keyboard.FocusedElement as UIElement;

            uiElement?.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, uiElement));
        }

        /// <summary>
        /// Performs a breadth-first search for the first enabled visible descendant of the specified visualTree that is focusable
        /// </summary>
        public static UIElement FindFirstFocusableChildElement(DependencyObject visualTree)
        {
            return visualTree.FindVisualDescendant<UIElement>(x => x.IsVisible && x.IsEnabled && x.Focusable);
        }

        /// <summary>
        /// Set the focus to the first thing we find in the visual tree that can have focus
        /// </summary>
        public static bool FocusFirstFocusableChildElement(DependencyObject visualTree, bool keyboardFocus = false)
        {
            UIElement element = FindFirstFocusableChildElement(visualTree);

            if (element == null)
            {
                Debug.Print("FocusHelper.TrySetFocus failed to find a visible focusable child control.");

                return false;
            }

            if (keyboardFocus)
            {
                IInputElement output = Keyboard.Focus(element);

                if (output == element)
                {
                    TextBox textBox = element as TextBox;

                    textBox?.SelectAll();

                    return true;
                }
            }

            return element.Focus();
        }

        /// <summary>
        /// Tries the set focus to the control with the specified name in the specified context.
        /// 
        /// NOTE: The focus isn't set immediately, instead a message is put in the current dispatcher's
        /// queue to set the focus during the "Loaded" event stage.
        /// </summary>
        /// <param name="visualTree">The root of the visual tree to search under, if null, all application windows will be searched</param>
        /// <param name="dataContext">The DataContext of the FrameworkElement in the visual tree to search for.</param>
        /// <param name="controlName">Name of the control to set focus to (must be Focusable or have a Focusable descendant)</param>
        /// <param name="keyboardFocus">if set to <c>true</c> the keyboard focus will be set, otherwise only the logical focus.</param>
        /// <returns>true if we were able to set the focus, otherwise false</returns>
        public static bool TrySetFocus(FrameworkElement visualTree, object dataContext, string controlName, bool keyboardFocus)
        {
            if (visualTree == null && dataContext == null)
                throw new InvalidOperationException("visualTree and dataContext cannot both be null");

            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

            Func<FrameworkElement, object, string, bool, bool> trySetFocusAsync = TrySetFocusInternal;

            object result = dispatcher.Invoke(trySetFocusAsync, new TimeSpan(0, 0, 0, 10), DispatcherPriority.Loaded,
                visualTree, dataContext, controlName, keyboardFocus);

            return result is bool && (bool)result;
        }

        private static bool TrySetFocusInternal(FrameworkElement visualTree, object dataContext, string controlName, bool keyboardFocus)
        {
            if (visualTree == null && dataContext == null)
                throw new InvalidOperationException("visualTree and dataContext cannot both be null");

            FrameworkElement contextVisual;

            if (visualTree == null)
            {
                contextVisual = FindFrameworkElementByDataContext(dataContext);
            }
            else if (dataContext != null)
            {
                contextVisual = visualTree.FindVisualDescendant<FrameworkElement>(x => x.DataContext == dataContext);
            }
            else
            {
                contextVisual = visualTree;
            }

            if (contextVisual == null)
            {
                Debug.Print("FocusHelper.TrySetFocus failed to find a FrameworkElement with a DataContext of {0} {1}",
                    dataContext.GetType(), dataContext);

                return false;
            }

            if (!string.IsNullOrEmpty(controlName))
            {
                contextVisual = contextVisual.FindVisualDescendant<FrameworkElement>(x => x.Name == controlName);

                if (contextVisual == null)
                {
                    Debug.Print(
                        "FocusHelper.TrySetFocus failed to find an element with the name '{0}' in the visual tree for the specified DataContext",
                        controlName);

                    return false;
                }
            }

            return FocusFirstFocusableChildElement(contextVisual);
        }

        private static FrameworkElement FindFrameworkElementByDataContext(object dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            Queue<DependencyObject> searchList = new Queue<DependencyObject>(
                Application.Current.Windows.OfType<DependencyObject>());

            while (searchList.Count > 0)
            {
                DependencyObject dependencyObject = searchList.Dequeue();

                FrameworkElement frameworkElement = dependencyObject as FrameworkElement;

                if (frameworkElement != null && frameworkElement.DataContext == dataContext)
                    return frameworkElement;

                int childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);

                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);

                    if (child != null) searchList.Enqueue(child);
                }
            }

            return null;
        }
    }
}
