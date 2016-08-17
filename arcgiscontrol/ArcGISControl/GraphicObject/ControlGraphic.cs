using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.GraphicObject
{
    public class ControlGraphic<T> : BaseGraphic
        where T : UIElement
    {
        private T control;

        public T Control
        {
            get { return this.control; }
        }

        public bool IsVisible
        {
            get
            {
                var visibility = this.Attributes["Visibility"];
                return visibility is Visibility
                    && (Visibility)visibility == Visibility.Visible;
            }
            set
            {
                this.Attributes["Visibility"] = value ?
                    Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ControlGraphic(T control, MapObjectType type, string id)
            : base(type, id)
        {
            this.control = control;
            this.Attributes["Control"] = control;
            this.IsVisible = true;
        }

        private void SetSymbolControlTemplate()
        {
            if (this.Symbol == null)
                return;

            var xaml =
@"<ControlTemplate
    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
    xmlns:g=""clr-namespace:ArcGISControl.GraphicObject;assembly=ArcGISControl""
    >
    <Grid
        Visibility=""{Binding Attributes[Visibility]}""
        >
        <Rectangle
            Name=""Element""
            Fill=""Transparent""
            />
        <g:ControlHost
            InnerControl=""{Binding Attributes[Control]}""
            Margin=""0,0,10,10""
            Width=""{Binding Clip.Bounds.Width, ElementName=Element}""
            Height=""{Binding Clip.Bounds.Height, ElementName=Element}""
            />
    </Grid>
</ControlTemplate>";
            this.Symbol.ControlTemplate = (ControlTemplate)XamlReader.Parse(xaml);
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            if (e.PropertyName == "Symbol")
            {
                this.SetSymbolControlTemplate();
            }
        }
    }
}
