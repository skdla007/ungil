
namespace ArcGISControl.ArcGISInternalHack
{
    using System.Reflection;
    using ESRI.ArcGIS.Client;

    internal static class GraphicsLayerTool
    {
        private static PropertyInfo GraphicsLayerDotCanvasPropertyInfo
            = typeof(GraphicsLayer).GetProperty("Canvas", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo LayerCanvasDotResetGeometryTransformsMethodInfo
            = Assembly.GetAssembly(typeof(Map))
                .GetType("ESRI.ArcGIS.Client.LayerCanvas")
                .GetMethod("ResetGeometryTransforms", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void ResetGeometryTransforms(this GraphicsLayer graphicsLayer)
        {
            var canvas = GraphicsLayerDotCanvasPropertyInfo.GetValue(graphicsLayer, new object[] { });
            LayerCanvasDotResetGeometryTransformsMethodInfo.Invoke(canvas, new object[] { });
        }
    }
}
