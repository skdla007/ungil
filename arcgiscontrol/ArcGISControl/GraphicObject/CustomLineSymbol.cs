using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace ArcGISControl.GraphicObject
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using ESRI.ArcGIS.Client.Symbols;
    using System.Windows.Media;
    using ArcGISControls.CommonData.Styles;
    using ArcGISControls.CommonData.Types;

    internal class CustomLineSymbol : LineSymbol
    {
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(int), typeof(CustomLineSymbol),
                                        new PropertyMetadata(default(int)));

        public int StrokeThickness
        {
            get { return (int)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty LineColorProperty =
             DependencyProperty.Register("LineColor", typeof(Brush), typeof(CustomLineSymbol), new PropertyMetadata(default(Brush)));

        public Brush LineColor
        {
            get { return (Brush)GetValue(LineColorProperty); }
            set { SetValue(LineColorProperty, value); }
        }

        public static readonly DependencyProperty DashArrayProperty =
            DependencyProperty.Register("DashArray", typeof(DoubleCollection), typeof(CustomLineSymbol),
                                        new PropertyMetadata(default(DoubleCollection)));

        public DoubleCollection DashArray
        {
            get { return (DoubleCollection)GetValue(DashArrayProperty); }
            set { SetValue(DashArrayProperty, value); }
        }


        public static readonly DependencyProperty StrokeCapProperty =
            DependencyProperty.Register("StrokeCap", typeof(PenLineCap), typeof(CustomLineSymbol),
                                        new PropertyMetadata(default(PenLineCap)));

        public PenLineCap StrokeCap
        {
            get { return (PenLineCap)GetValue(StrokeCapProperty); }
            set { SetValue(StrokeCapProperty, value); }
        }

        public static readonly DependencyProperty LineJoinProperty =
            DependencyProperty.Register("LineJoin", typeof(PenLineJoin), typeof(CustomLineSymbol),
                                        new PropertyMetadata(default(PenLineJoin)));

        public PenLineJoin LineJoin
        {
            get { return (PenLineJoin)GetValue(LineJoinProperty); }
            set { SetValue(LineJoinProperty, value); }
        }

        public static readonly DependencyProperty LineStrokeTypeProperty =
            DependencyProperty.Register("LineStrokeType", typeof(LineStrokeType), typeof(CustomLineSymbol),
                                        new PropertyMetadata(default(LineStrokeType), PropertyChangedCallback));

        public LineStrokeType LineStrokeType
        {
            get { return (LineStrokeType)GetValue(LineStrokeTypeProperty); }
            set { SetValue(LineStrokeTypeProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var obj = dependencyObject as CustomLineSymbol;

            Debug.Assert(obj != null);

            if (dependencyPropertyChangedEventArgs.Property == LineStrokeTypeProperty)
            {
                var dataInfo = dependencyPropertyChangedEventArgs.NewValue is LineStrokeType ? (LineStrokeType)dependencyPropertyChangedEventArgs.NewValue : (LineStrokeType)0;

                //if (dataInfo != null)
                //{
                    var style = new LineStrokeStyle(dataInfo);

                    obj.StrokeCap = style.PenLineCap;
                    obj.DashArray = style.DashArray;
                //}
            }
        }

        public CustomLineSymbol()
        {
            var xamlTemplate =
                "<ControlTemplate  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">           " +
                "<Grid>                           " +
                "<Path  Name=\"Element\"      " +
                    "StrokeDashArray=\"{Binding Symbol.DashArray, Mode=TwoWay}\"        " +
                    "Stroke=\"{Binding Symbol.LineColor, Mode=TwoWay}\"         " +
                    "StrokeThickness=\"{Binding Symbol.StrokeThickness, Mode=TwoWay}\"      " +
                    "StrokeLineJoin=\"{Binding Symbol.LineJoin, Mode=TwoWay}\"          " +
                    "StrokeEndLineCap=\"{Binding Symbol.StrokeCap, Mode=TwoWay}\"       " +
                    "StrokeDashCap=\"{Binding Symbol.StrokeCap, Mode=TwoWay}\"      " +
                    "StrokeStartLineCap=\"{Binding Symbol.StrokeCap, Mode=TwoWay}\" />        " +
                "</Grid> " +
                "</ControlTemplate>                  ";

            this.ControlTemplate = (ControlTemplate)XamlReader.Parse(xamlTemplate);
        }
    }
}
