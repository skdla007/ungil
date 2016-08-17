using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.UIControl
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class NumericTextBoxControl : TextBox
    {
        #region Construct

        public NumericTextBoxControl()
        {
            InputMethod.SetIsInputMethodEnabled(this, false);

            this.PreviewKeyDown += NumericTextBoxControl_PreviewKeyDown;
            this.PreviewTextInput += NumericTextBoxControl_PreviewTextInput;
            DataObject.AddPastingHandler(this, NumericTextBoxControl_OnPaste);

            this.Unloaded += NumericTextBoxControl_Unloaded;
        }

        void NumericTextBoxControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.PreviewKeyDown -= NumericTextBoxControl_PreviewKeyDown;
            this.PreviewTextInput -= NumericTextBoxControl_PreviewTextInput;
            DataObject.RemovePastingHandler(this, NumericTextBoxControl_OnPaste);

            this.Unloaded -= NumericTextBoxControl_Unloaded;
        }

        #endregion

        #region Properties

        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        #endregion

        #region Methods

        #region Event Handlers

        void NumericTextBoxControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space; // doesn't allow space in textBox
        }

        private void NumericTextBoxControl_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            var text = e.SourceDataObject.GetData(DataFormats.Text) as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                e.CancelCommand();
                return;
            }

            if (!this.AreAllValidNumericChars(text))
            {
                e.CancelCommand();
                return;
            }

            if (text.StartsWith("0"))
            {
                e.CancelCommand();
                return;
            }

            // 숫자만 들어온 경우 MaxValue보다 크면 MaxValue로 할당한다.
            if (this.AreAllValidNumericChars(text))
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var inputNumber = int.Parse(text);
                    if (inputNumber > this.MaxValue)
                    {
                        textBox.Text = this.MaxValue.ToString();
                        e.CancelCommand();
                        return;
                    }
                }
            }
        }

        private void NumericTextBoxControl_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }

            if (string.CompareOrdinal(e.Text, ".") == 0)
            {
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                ((TextBox)sender).MoveFocus(request);
            }

            e.Handled = !this.AreAllValidNumericChars(e.Text);
            //e.Handled = this.ValidateInputIpAddress(textBox, e.Text); 
        }

        #endregion

        private bool ValidateInputIpAddress(TextBox textBox, string inputText)
        {
            // 커서 위치 index를 찾는다.
            var position = textBox.SelectionStart;

            var text =
                !string.IsNullOrWhiteSpace(textBox.SelectedText) ?
                    this.GetTextExceptSelection(textBox.Text, textBox.SelectedText, position) :
                    textBox.Text;

            // 앞뒤 문자열을 자른 후 inputText를 연결해 MaxValue와 비교한다.
            var preChar = text.Substring(0, position);
            var postChar = text.Substring(position, text.Length - position);
            var combinedChar = preChar + inputText + postChar;

            if (!this.AreAllValidNumericChars(combinedChar))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                if (combinedChar.StartsWith("0"))
                {
                    if (position > 0)
                    {
                        return true;
                    }

                    if (string.CompareOrdinal(inputText, "0") == 0)
                    {
                        return true;
                    }
                }
            }

            // MaxValue보다 크면 적용하지 않는다.
            var targetValue = int.Parse(combinedChar);

            //if (targetValue > this.MaxValue)
            //{
            //    textBox.Text = this.MaxValue.ToString();
            //}

            return targetValue > this.MaxValue;
        }

        private string GetTextExceptSelection(string wholeText, string selectionText, int cursorPosition)
        {
            var selectionTextLength = selectionText.Length;
            return wholeText.Remove(cursorPosition, selectionTextLength);
        }

        /// <summary>
        /// Number Check
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool AreAllValidNumericChars(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            var regex = new Regex("[^0-9]+"); //regex that matches disallowed text
            return !regex.IsMatch(str);
        }

        #endregion
    }
}
