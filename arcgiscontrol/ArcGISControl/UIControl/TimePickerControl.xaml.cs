
using System;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ArcGISControl.UIControl
{
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for TimePickerControl.xaml
    /// </summary>
    public partial class TimePickerControl : UserControl
    {
        public TimePickerControl()
        {
            InitializeComponent();

            this.Focusable = true;

            this.MouseLeftButtonDown += TimePickerControl_MouseLeftButtonDown;
            this.MouseLeftButtonUp += TimePickerControl_MouseLeftButtonUp;

            this.xCalendar.PreviewMouseUp += xCalendar_PreviewMouseUp;

            this.xGoButton.Click += xGoButton_Click;

            this.xCalendar.SelectionMode = CalendarSelectionMode.SingleDate;
        }

        public event EventHandler eGoButtonClicked;

        protected virtual void OnEGoButtonClicked()
        {
            var handler = eGoButtonClicked;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void xGoButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.OnEGoButtonClicked();
        }

        private void xCalendar_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured is CalendarItem)
            {
                Mouse.Capture(null);
            }
        }

        private void TimePickerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void TimePickerControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public void SetDateTiem(DateTime aDateTime)
        {
            this.xCalendar.SelectedDate = aDateTime;
            this.xTimePicker.SelectedTime = aDateTime.TimeOfDay;
        }

        public DateTime GetDateTime()
        {
            var newDateTime = this.xCalendar.SelectedDate == null ? DateTime.Now.Date : this.xCalendar.SelectedDate.Value.Date;

            newDateTime = newDateTime.Add(this.xTimePicker.SelectedTime);

            return newDateTime;
        }
    }
}
