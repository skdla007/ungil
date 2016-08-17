using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;

namespace ArcGISControl.UIControl.GraphicObjectControl
{
    /// <summary>
    /// Interaction logic for TextBoxControl.xaml
    /// </summary>
    public partial class TextBoxControl : UserControl
    {
        private bool _IsEditMode = false;

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextBoxControl));

        public bool IsReadOnly
        {
            get { return (bool)this.GetValue(IsReadOnlyProperty); }
            set
            {
                this.SetValue(IsReadOnlyProperty, value);

                this.xTextBox.Cursor = this.IsReadOnly
                                           ? CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.HandOpen)
                                           : CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver);
            }
        }

        public BaseMapTextObjectInfo DataInfo
        {
            get
            {
                return this.DataContext as BaseMapTextObjectInfo;
            }
            set
            {
                this.DataContext = value;
            }
        }

        public bool IsPanning { get; set; }

        public TextBoxControl()
        {
            InitializeComponent();

            this.Focusable = false;

            this.xControlWrappingViewbox.MouseLeftButtonDown += xControlWrappingViewbox_MouseLeftButtonDown;
            this.xTextBox.LostFocus += this.TextBox_LostFocus;
            this.xTextBox.MouseEnter += this.xTextBox_MouseEnter;
            this.xTextBox.Loaded += xTextBox_Loaded;
        }

        void xControlWrappingViewbox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsPanning) return;

            if (e.ClickCount >= 2)
            {
                this.SetEditModeTextBox();
            }
        }

        void xTextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            var TextBoxControl = (TextBox)sender;
            if (TextBoxControl == null) return;

            TextBoxControl.SelectionStart = TextBoxControl.Text.Length;
            TextBoxControl.SelectionLength = 0;
        }

        private void xTextBox_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.xTextBox.Focus();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == UIElement.FocusableProperty)
            {
                if ((bool)e.NewValue)
                {
                    this.xTextBox.Cursor = Cursors.IBeam;
                }
                else
                {
                    this.xTextBox.Cursor = CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver);
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            if (textBox.IsMouseOver)
                return;

            IsPanning = false;
            this.Focusable = false;
        }

        /// <summary>
        /// TextBox의 Size를 컨트롤과 같은 크기로 맞춰준다.
        /// </summary>
        public void FitTextBoxSize()
        {
            this.DataInfo.TextBoxSize = new Size(this.ActualWidth, this.ActualHeight);
        }

        public bool IsEditingTextBox()
        {
            if (this.xTextBox.IsReadOnly == false &&
                this.xTextBox.Focusable == true &&
                this.xTextBox.Cursor == Cursors.IBeam)
                return true;

            return false;
        }

        /// <summary>
        /// TextBox의 Focus를 제거 한다
        /// </summary>
        public void TakeTextBoxFocus()
        {
            if (this.IsReadOnly) return;

            IsPanning = false;
            _IsEditMode = false;
            this.xTextBox.IsReadOnly = true;
            this.xTextBox.CaretIndex = 0;
            this.xTextBox.Focusable = false;
            this.xTextBox.Cursor = CursorManager.Instance.GetCursor(ArcGISControl.Helper.CursorType.MoveOver);
        }

        /// <summary>
        /// Text Box의 focus를 설정한다.
        /// </summary>
        public void SetMoveModeTextBox()
        {
            if (this.IsReadOnly) return;

            if (_IsEditMode)
            {
                this.xTextBox.Focusable = true;
                this.xTextBox.IsReadOnly = false;
                this.xTextBox.Focus();
                this.xTextBox.Cursor = Cursors.IBeam;
            }
            else
            {
                this.xControlWrappingViewbox.Focus();
            }

            //this.Focusable = true;
            IsPanning = false;
        }

        public void SetEditModeTextBox()
        {
            if (this.IsReadOnly) return;

            IsPanning = false;
            _IsEditMode = true;
            this.xTextBox.Focusable = true;
            this.xTextBox.IsReadOnly = false;
            this.xTextBox.Focus();
            this.xTextBox.SelectAll();
            this.xTextBox.Cursor = Cursors.IBeam;
        }
    }
}
