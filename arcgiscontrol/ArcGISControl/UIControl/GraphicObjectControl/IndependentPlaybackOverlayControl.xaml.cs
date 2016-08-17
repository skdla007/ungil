
namespace ArcGISControl.UIControl.GraphicObjectControl
{
    using System;
    using System.Windows;
    using System.Globalization;
    using System.Windows.Input;
    using System.Windows.Media;
    using Helper;

    /// <summary>
    /// Interaction logic for IndependentPlaybackOverlayControl.xaml
    /// </summary>
    public partial class IndependentPlaybackOverlayControl
    {
        private DateTime playPosition;

        private bool enablePlayback;

        private readonly SolidColorBrush dateNormalForgroundBrush = BrushUtil.ConvertFromString("#FFE4E4E4") as SolidColorBrush;
        private readonly SolidColorBrush dateMouseOverForgroundBrush = BrushUtil.ConvertFromString("#FFFFAF00") as SolidColorBrush;

        public IndependentPlaybackOverlayControl()
        {
            InitializeComponent();

            this.xSpeedSlider.Minimum = -4;
            this.xSpeedSlider.Maximum = 4;
            this.xSpeedSlider.Value = 0;
            this.xSpeedSlider.SmallChange = 0.1;

            this.PlayPosition = DateTime.Now;

            this.xAlertBroadcastPopupButton.Click += AlertBroadcastPopupButton_Click;
            this.xStartIndependentModeButton.Click += xStartIndependentModeButton_Click;
            this.xEndIndependentModeButton.Click += this.xEndInstantPlaybackModeButton_Click;

            this.xRewindToggleButton.Click += this.xRewindToggleButton_Click;
            this.xPlayToggleButton.Click += this.xPlayToggleButton_Click;

            this.xSpeedSlider.ValueChanged += this.xSpeedSlider_ValueChanged;

            this.xDateGrid.MouseLeftButtonUp += xDateGrid_MouseLeftButtonUp;

            this.IsVisibleChanged += IndependentPlaybackOverlayControl_IsVisibleChanged;

            this.xTimePickerControl.xGoButton.Click += xGoButton_Click;

            this.xDateGrid.MouseEnter += xDateGrid_MouseEnter;
            this.xDateGrid.MouseLeave += xDateGrid_MouseLeave;
        }

        #region Event

        public event EventHandler eStartIndependentPlayback;

        private void OnEStartIndependentPlayback(EventArgs e)
        {
            var handler = this.eStartIndependentPlayback;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler eEndIndependentPlayback;

        private void OnEEndIndependentPlayback(EventArgs e)
        {
            var handler = this.eEndIndependentPlayback;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler ePlayButtonClicked;

        public void OnEPlayButtonClicked(EventArgs e)
        {
            var handler = this.ePlayButtonClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler eRewindButtonClicked;

        public void OnERewindButtonClicked(EventArgs e)
        {
            var handler = this.eRewindButtonClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler ePauseButtonClicked;

        public void OnEPauseButtonClicked(EventArgs e)
        {
            var handler = this.ePauseButtonClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ValueChangedEventArgs> eSpeedChanged;

        public void OnESpeedChanged(ValueChangedEventArgs e)
        {
            var handler = this.eSpeedChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler eStateChanged;

        public void OnEeStateChanged(EventArgs e)
        {
            var handler = this.eStateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler eGoButtonClicked;

        public void OnEGoButtonClicked(EventArgs e)
        {
            var handler = this.eGoButtonClicked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> eAlertBroadcastCapture;

        public void OnEAlertBroadcastCapture(EventArgs e)
        {
            var handler = this.eAlertBroadcastCapture;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> ePlayTimeChanged;

        protected virtual void OnEPlayTimeChanged(EventArgs e)
        {
            var handler = this.ePlayTimeChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion // Event


        #region Event Handler

        private void xDateGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.xDateTextBlock.Foreground = dateNormalForgroundBrush;
            this.xTimeTextBlock.Foreground = dateNormalForgroundBrush;
        }

        private void xDateGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            this.xDateTextBlock.Foreground = dateMouseOverForgroundBrush;
            this.xTimeTextBlock.Foreground = dateMouseOverForgroundBrush;
        }

        private void xGoButton_Click(object sender, RoutedEventArgs e)
        {
            this.xTimePickerControlPopup.IsOpen = false;

            if (this.xTimePickerControl.xCalendar.SelectedDate == null)
            {
                return;
            }

            var playDate = this.xTimePickerControl.xCalendar.SelectedDate.Value.Date;

            this.PlayPosition = playDate.Add(this.xTimePickerControl.xTimePicker.SelectedTime);

            this.OnEGoButtonClicked(new EventArgs());
        }

        private void IndependentPlaybackOverlayControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                this.xTimePickerControlPopup.IsOpen = false;
            }
        }

        private void xDateGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.xTimePickerControlPopup.IsOpen)
            {
                this.xTimePickerControlPopup.IsOpen = true;

                this.xTimePickerControl.xCalendar.SelectedDate = this.PlayPosition;
                this.xTimePickerControl.xCalendar.DisplayDate = this.playPosition;

                this.xTimePickerControl.xTimePicker.SelectedTime = this.playPosition.TimeOfDay;
            }
            else
            {
                this.xTimePickerControlPopup.IsOpen = false;
            }
        }

        private void xStartIndependentModeButton_Click(object sender, RoutedEventArgs e)
        {
            this.xRewindToggleButton.IsChecked = false;
            this.xPlayToggleButton.IsChecked = false;

            this.EnablePlayback = true;

            this.PlayPosition = DateTime.Now.AddMinutes(-1);

            this.OnEStartIndependentPlayback(new EventArgs());

            this.OnEeStateChanged(new EventArgs());
        }

        private void xEndInstantPlaybackModeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.xRewindToggleButton.IsChecked = false;
            this.xPlayToggleButton.IsChecked = false;

            this.EnablePlayback = false;

            this.OnEEndIndependentPlayback(new EventArgs());

            this.OnEeStateChanged(new EventArgs());
        }

        private void xPlayToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.xRewindToggleButton.IsChecked = false;

            if (this.xPlayToggleButton.IsChecked == true)
            {
                this.OnEPlayButtonClicked(new EventArgs());
            }
            else
            {
                this.OnEPauseButtonClicked(new EventArgs());
            }

            this.OnEeStateChanged(new EventArgs());
        }

        void AlertBroadcastPopupButton_Click(object sender, RoutedEventArgs e)
        {
            // playback 멈춤.
            this.xRewindToggleButton.IsChecked = false;
            this.xPlayToggleButton.IsChecked = false;
            this.OnEPauseButtonClicked(new EventArgs());

            // 상태 변경 날림.
            this.OnEeStateChanged(new EventArgs());

            // AlertBroadcast 날림.
            this.OnEAlertBroadcastCapture(new EventArgs());
        }

        private void xRewindToggleButton_Click(object sender, RoutedEventArgs e)
        {
            this.xPlayToggleButton.IsChecked = false;

            if (this.xRewindToggleButton.IsChecked == true)
            {
                this.OnERewindButtonClicked(new EventArgs());
            }
            else
            {
                this.OnEPauseButtonClicked(new EventArgs());
            }

            this.OnEeStateChanged(new EventArgs());
        }

        private void xSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var speed = Math.Round(e.NewValue, 1);

            this.xSpeedSlider.ToolTip = speed.ToString(CultureInfo.InvariantCulture);

            if (Math.Abs(speed - 0) < 0.1)
            {
                this.xPlayToggleButton.IsChecked = false;
                this.xRewindToggleButton.IsChecked = false;
            }
            else if (speed < -0.1)
            {
                this.xPlayToggleButton.IsChecked = false;
                this.xRewindToggleButton.IsChecked = true;
            }
            else if (speed > 0.1)
            {
                this.xRewindToggleButton.IsChecked = false;
                this.xPlayToggleButton.IsChecked = true;
            }

            this.OnESpeedChanged(new ValueChangedEventArgs(speed));

            this.OnEeStateChanged(new EventArgs());
        }

        #region Propertis

        public string SyncId { get; set; }

        public bool EnablePlayback
        {
            get
            {
                return enablePlayback;
            }

            set
            {
                enablePlayback = value;

                if (value)
                {
                    this.xStartIndependentModeButton.Visibility = Visibility.Collapsed;
                    this.xEndIndependentModeButton.Visibility = Visibility.Visible;
                    this.xVideoControlGrid.Visibility = Visibility.Visible;
                    this.xAlertBroadcastPopupButton.Visibility = Visibility.Visible;

                    //this.OnEStartIndependentPlayback(new EventArgs());
                }
                else
                {
                    this.xStartIndependentModeButton.Visibility = Visibility.Visible;
                    this.xEndIndependentModeButton.Visibility = Visibility.Collapsed;
                    this.xVideoControlGrid.Visibility = Visibility.Collapsed;
                    this.xAlertBroadcastPopupButton.Visibility = Visibility.Collapsed;

                    //this.OnEEndIndependentPlayback(new EventArgs());
                }
            }
        }

        public DateTime PlayPosition
        {
            get
            {
                return this.playPosition;
            }

            set
            {
                var changed = value != this.PlayPosition;

                this.playPosition = value;

                this.xDateTextBlock.Text = value.ToString("d", CultureInfo.CurrentCulture);
                this.xTimeTextBlock.Text = value.ToString("tt h:mm:ss.f", CultureInfo.CurrentCulture);

                if (changed)
                    this.OnEPlayTimeChanged(new EventArgs());
            }
        }

        #endregion // Propertis

        #endregion // Event Handler


        #region Methods

        #endregion // Methods
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(int aVlaue)
        {
            this.IntValue = aVlaue;
        }

        public ValueChangedEventArgs(double aVlaue)
        {
            this.DoubleValue = aVlaue;
        }

        public int? IntValue { get; set; }

        public double? DoubleValue { get; set; }
    }
}
