using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.UIControl
{
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    public class UpDownButton : ButtonBase
    {
        public bool PressButton
        {
            set { this.IsPressed = value; }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Return)
            {
                this.IsPressed = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.Key == Key.Return)
            {
                this.IsPressed = false;
            }
        }
    }
}
