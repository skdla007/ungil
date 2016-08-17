using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISControl.UIControl
{
    /// <summary>
    /// NumericUpDownControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NumericUpDownControl
    {
        private readonly Regex numMatchExpression;

        public NumericUpDownControl()
        {
            InitializeComponent();
            InputMethod.SetIsInputMethodEnabled(this.TextBoxValue, false);

            numMatchExpression = new Regex(@"^(0|-?[1-9]\d*)$");
            TextBoxValue.Text = "0";
        }

        private void ResetText(TextBox tb)
        {
            tb.Text = 0 < Minimum ? Minimum.ToString(CultureInfo.InvariantCulture) : "0";

            tb.SelectAll();
        }

        private void value_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var text = tb.Text;
            text = text.Remove(tb.SelectionStart, tb.SelectionLength);
            text = text.Insert(tb.SelectionStart, e.Text);

            e.Handled = !numMatchExpression.IsMatch(text);
        }

        private void value_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (!numMatchExpression.IsMatch(tb.Text)) ResetText(tb);

            int parsedValue;

            try
            {
                parsedValue = Convert.ToInt32(tb.Text);
            }
            catch (OverflowException)
            {
                if (tb.Text.Length > 0 && tb.Text[0] == '-')
                    parsedValue = Minimum;
                else
                    parsedValue = Maximum;

                // 강제로 갱신해준다.
                if (Value == parsedValue)
                {
                    this.TextBoxValue.Text = parsedValue.ToString(CultureInfo.InvariantCulture);
                }
            }

            if (Value != parsedValue)
            {
                Value = parsedValue;
                if (Value < Minimum) Value = Minimum;
                if (Value > Maximum) Value = Maximum;
            }

            RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }

        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            if (Value < Maximum)
            {
                Value++;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
        }

        private void Decrease_Click(object sender, RoutedEventArgs e)
        {
            if (Value > Minimum)
            {
                Value--;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }

        /// <summary>The Value property represents the TextBoxValue of the control.</summary>
        /// <returns>The current TextBoxValue of the control</returns>      

        public int Value
        {
            get
            {
                return (int)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDownControl),
              new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSomeValuePropertyChanged));


        private static void OnSomeValuePropertyChanged(
        DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var numericBox = target as NumericUpDownControl;
            Debug.Assert(numericBox != null, "numericBox != null");
            numericBox.TextBoxValue.Text = ((int)e.NewValue).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Maximum value for the Numeric Up Down control
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata(int.MaxValue));

        /// <summary>
        /// Minimum value of the numeric up down conrol.
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(NumericUpDownControl), new UIPropertyMetadata(0));


        // Value changed
        private static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericUpDownControl));

        /// <summary>The ValueChanged event is called when the TextBoxValue of the control changes.</summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        //Increase button clicked
        private static readonly RoutedEvent IncreaseClickedEvent =
            EventManager.RegisterRoutedEvent("IncreaseClicked", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericUpDownControl));

        /// <summary>The IncreaseClicked event is called when the Increase button clicked</summary>
        public event RoutedEventHandler IncreaseClicked
        {
            add { AddHandler(IncreaseClickedEvent, value); }
            remove { RemoveHandler(IncreaseClickedEvent, value); }
        }

        //Increase button clicked
        private static readonly RoutedEvent DecreaseClickedEvent =
            EventManager.RegisterRoutedEvent("DecreaseClicked", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(NumericUpDownControl));

        /// <summary>The DecreaseClicked event is called when the Decrease button clicked</summary>
        public event RoutedEventHandler DecreaseClicked
        {
            add { AddHandler(DecreaseClickedEvent, value); }
            remove { RemoveHandler(DecreaseClickedEvent, value); }
        }

        /// <summary>
        /// Checking for Up and Down events and updating the value accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void value_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (e.IsDown && e.Key == Key.Up && Value < Maximum)
            {
                Value++;
                RaiseEvent(new RoutedEventArgs(IncreaseClickedEvent));
            }
            else if (e.IsDown && e.Key == Key.Down && Value > Minimum)
            {
                Value--;
                RaiseEvent(new RoutedEventArgs(DecreaseClickedEvent));
            }
        }
    }
}
