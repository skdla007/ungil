using System.Windows;

namespace ArcGISControl.Helper
{
    public static class UIElementScreenExtension
    {
        public static Point PointToScreenDIU(this UIElement element, Point point)
        {
            var result = element.PointToScreen(point);
            var compositionTarget = PresentationSource.FromVisual(element).CompositionTarget;
            return compositionTarget.TransformFromDevice.Transform(result);
        }
    }
}
