using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.GraphicObject
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Markup;
    using ESRI.ArcGIS.Client.Geometry;
    using ESRI.ArcGIS.Client.Symbols;

    class TrailSymbol : LineSymbol
    {
        public static readonly DependencyProperty ExtentProperty =
            DependencyProperty.Register("Extent", typeof(Envelope), typeof(TrailSymbol), new PropertyMetadata(default(Envelope)));

        public Envelope Extent
        {
            get { return (Envelope)GetValue(ExtentProperty); }
            set { SetValue(ExtentProperty, value); }
        }

        public static readonly DependencyProperty TrailPointsProperty =
            DependencyProperty.Register("TrailPoints", typeof(PointCollection), typeof(TrailSymbol), new PropertyMetadata(default(PointCollection)));

        public PointCollection TrailPoints
        {
            get { return (PointCollection)GetValue(TrailPointsProperty); }
            set { SetValue(TrailPointsProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(TrailSymbol), new PropertyMetadata(default(double)));

        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }


        /// <summary>
        /// Initializes a new instance of the TrailSymbol class.
        /// </summary>
        public TrailSymbol()
        {
            var xamlTemplate =
                "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'          "+
                " xmlns:g='clr-namespace:ArcGISControl.GraphicObject;assembly=ArcGISControl'>                     "+
                "<Grid>                           "+
                "<Rectangle Name='Element'/>              "+
                "<g:Trail ActualGeometry='{Binding Clip, ElementName=Element}'            "+
                "         TrailPoints='{Binding Symbol.TrailPoints, Mode=TwoWay}'       " +
                "         Extent='{Binding Symbol.Extent, Mode=TwoWay}'       " +
                "         Progress='{Binding Symbol.Progress, Mode=TwoWay}'       " +
                "/>                                   " +
                "</Grid>                          "+
                "</ControlTemplate>                  ";


            this.ControlTemplate = (ControlTemplate) XamlReader.Parse(xamlTemplate);
        }
    }
}
