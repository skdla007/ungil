namespace ArcGISControl.ArcGISInternalHack
{
    using ESRI.ArcGIS.Client;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;

    internal class GraphicTool
    {
        public static List<Graphic> GetGraphicsFromPoint(Visual startObject, Point pointInObject)
        {
            if (startObject == null)
                return null;

            var graphicElementType = Type.GetType("ESRI.ArcGIS.Client.Symbols.GraphicElement,ESRI.ArcGIS.Client");
            if (graphicElementType == null)
                throw new TypeLoadException("ArcGIS type 'GraphicElement' not found");

            var graphicProperty = graphicElementType.GetProperty("Graphic", BindingFlags.Instance | BindingFlags.NonPublic);

            if (graphicProperty == null)
                throw new TypeLoadException("'Graphic' property is not in 'GraphicElement'");

            var ret = new List<Graphic>();
            var visitCheck = new HashSet<object>();

            VisualTreeHelper.HitTest(startObject, null, testResult =>
            {
                var currentObject = testResult.VisualHit;

                while (currentObject != null && !ReferenceEquals(currentObject, startObject))
                {
                    if (!visitCheck.Add(currentObject))
                        break;

                    if (graphicElementType.IsInstanceOfType(currentObject))
                    {
                        var graphic = graphicProperty.GetValue(currentObject, null) as Graphic;
                        if (graphic != null)
                            ret.Add(graphic);
                    }

                    currentObject = VisualTreeHelper.GetParent(currentObject);
                }

                return HitTestResultBehavior.Continue;
            }, new PointHitTestParameters(pointInObject));

            return ret;
        }
    }
}
