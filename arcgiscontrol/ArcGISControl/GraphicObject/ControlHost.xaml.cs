using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISControl.GraphicObject
{
    public partial class ControlHost : Decorator
    {
        internal static DependencyProperty InnerControlProperty =
            DependencyProperty.Register(
                "InnerControl",
                typeof(UIElement),
                typeof(ControlHost),
                new PropertyMetadata(
                    (d, e) =>
                    {
                        var instance = d as ControlHost;
                        if (instance == null) return;
                        var control = e.NewValue as UIElement;
                        instance.Child = control;
                    }
                )
            );

        public UIElement InnerControl
        {
            get { return (UIElement)this.GetValue(InnerControlProperty); }
            set { this.SetValue(InnerControlProperty, value); }
        }

        public ControlHost()
        {
            InitializeComponent();
        }
    }
}
