namespace VisualTreeHelper.WPF
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    /// Extension methods for DependencyObjects
    /// used for walking the visual tree with
    /// LINQ expressions.
    /// These simplify using VisualTreeHelper to one line calls.
    /// </summary>
    public static class VisualTreeHelperExtensions
    {
        /// <summary>
        /// Gets the window root that is the top level ascendant of the window.Content.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static UIElement GetRealWindowRoot(Window window = null)
        {
            if (window == null)
            {
                window = Application.Current.MainWindow;
            }

            if (window == null)
            {
                return null;
            }

            var root = window.Content as FrameworkElement;

            if (root != null)
            {
                var ancestors = root.GetAncestors().ToList();

                if (ancestors.Count > 0)
                {
                    root = (FrameworkElement)ancestors[ancestors.Count - 1];
                }
            }

            return root;
        }

        /// <summary>
        /// Gets the first descendant that is of the given type.
        /// </summary>
        /// <remarks>
        /// Returns null if not found.
        /// </remarks>
        /// <typeparam name="T">Type of descendant to look for.</typeparam>
        /// <param name="start">The start object.</param>
        /// <returns></returns>
        public static T GetFirstDescendantOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendantsOfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the descendants of the given type.
        /// </summary>
        /// <typeparam name="T">Type of descendants to return.</typeparam>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public static IEnumerable<T> GetDescendantsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetDescendants().OfType<T>();
        }

        /// <summary>
        /// Gets the descendants.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetDescendants(this DependencyObject start)
        {
            if (start == null)
            {
                yield break;
            }

            var queue = new Queue<DependencyObject>();

            var popup = start as Popup;

            if (popup != null)
            {
                if (popup.Child != null)
                {
                    queue.Enqueue(popup.Child);
                    yield return popup.Child;
                }
            }
            else
            {
                var count = VisualTreeHelper.GetChildrenCount(start);

                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(start, i);
                    queue.Enqueue(child);
                    yield return child;
                }
            }

            while (queue.Count > 0)
            {
                var parent = queue.Dequeue();

                popup = parent as Popup;

                if (popup != null)
                {
                    if (popup.Child != null)
                    {
                        queue.Enqueue(popup.Child);
                        yield return popup.Child;
                    }
                }
                else
                {
                    var count = VisualTreeHelper.GetChildrenCount(parent);

                    for (int i = 0; i < count; i++)
                    {
                        var child = VisualTreeHelper.GetChild(parent, i);
                        yield return child;
                        queue.Enqueue(child);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the child elements.
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetChildren(this DependencyObject parent)
        {
            var popup = parent as Popup;

            if (popup?.Child != null)
            {
                yield return popup.Child;
                yield break;
            }

            var count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;
            }
        }

        /// <summary>
        /// Gets the child elements sorted in render order (by ZIndex first, declaration order second).
        /// </summary>
        /// <param name="parent">The parent element.</param>
        /// <returns></returns>
        public static IEnumerable<DependencyObject> GetChildrenByZIndex(this DependencyObject parent)
        {
            int i = 0;
            var indexedChildren =
                parent.GetChildren().Cast<FrameworkElement>().Select(
                child => new { Index = i++, ZIndex = Canvas.GetZIndex(child), Child = child });

            return
                from indexedChild in indexedChildren
                orderby indexedChild.ZIndex, indexedChild.Index
                select indexedChild.Child;
        }

        /// <summary>
        /// Gets the first ancestor that is of the given type.
        /// </summary>
        /// <remarks>
        /// Returns null if not found.
        /// </remarks>
        /// <typeparam name="T">Type of ancestor to look for.</typeparam>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        public static T GetFirstAncestorOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetAncestorsOfType<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the ancestors of a given type, starting with parent and going towards the visual tree root.
        /// </summary>
        /// <typeparam name="T">Type of ancestor to look for.</typeparam>
        /// <param name="start">The start.</param>
        /// <returns>The ancestors of a given type, starting with parent and going towards the visual tree root.</returns>
        public static IEnumerable<T> GetAncestorsOfType<T>(this DependencyObject start) where T : DependencyObject
        {
            return start.GetAncestors().OfType<T>();
        }

        /// <summary>
        /// Gets the ancestors, starting with parent and going towards the visual tree root.
        /// </summary>
        /// <param name="start">The starting element.</param>
        /// <returns>The ancestor elements, starting with parent and going towards the visual tree root.</returns>
        public static IEnumerable<DependencyObject> GetAncestors(this DependencyObject start)
        {
            var parent = VisualTreeHelper.GetParent(start);

            while (parent != null)
            {
                yield return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        /// <summary>
        /// Gets the siblings, including the start element.
        /// </summary>
        /// <param name="start">The start element.</param>
        /// <returns>The siblings, including the start element.</returns>
        public static IEnumerable<DependencyObject> GetSiblings(this DependencyObject start)
        {
            var parent = VisualTreeHelper.GetParent(start);

            if (parent == null)
            {
                yield return start;
            }
            else
            {
                var count = VisualTreeHelper.GetChildrenCount(parent);

                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    yield return child;
                }
            }
        }

    }
}
