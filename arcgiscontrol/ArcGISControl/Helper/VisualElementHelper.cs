
namespace ArcGISControl.Helper
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Media3D;
    
    public class VisualElementHelper
    {
        public static DependencyObject FindChildControl<T>(object element)
        {
            var control = element as DependencyObject;
            if (control == null)
            {
                return null;
            }

            var childNumber = VisualTreeHelper.GetChildrenCount(control);
            for (var i = 0; i < childNumber; i++)
            {
                var child = VisualTreeHelper.GetChild(control, i);
                if (child != null && child is T)
                {
                    return child;
                }
                else
                {
                    FindChildControl<T>(child);
                }
            }
            return null;
        }

        public static DependencyObject FindVisualTreeRoot(DependencyObject d)
        {
            var current = d;
            var result = d;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                    break;

                current = LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

        public static DependencyObject FindParentControl<T>(object element)
        {
            var control = element as DependencyObject;
            if (control == null)
            {
                return null;
            }

            control = FindVisualTreeRoot(control);

            var parent = VisualTreeHelper.GetParent(control);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }
    }
}
