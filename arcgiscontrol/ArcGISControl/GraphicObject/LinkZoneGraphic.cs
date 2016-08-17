using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Symbols;
using ArcGISControl.Helper;

namespace ArcGISControl.GraphicObject
{
    /// <summary>
    /// Work station 도 이 객체를 쓰고 있음 
    /// 이름을 바꿔야 겠음 
    /// </summary>
    public class LinkZoneGraphic : PolygonGraphic
    {
        #region Construction

        //Brightness 되기전 색상 저장
        private SolidColorBrush origianlFillColor;
        private SolidColorBrush originalBorderColor;
        private bool isSelectedBySplunk = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointCollection"></param>
        /// <param name="color"></param>
        /// <param name="borderColor"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public LinkZoneGraphic(List<Point> pointCollection, Color color, Color borderColor, MapObjectType type, string id)
            : base(pointCollection, color, borderColor, type, id)
        {

        }

        #endregion // Construcstion

        #region Method

        /// <summary>
        /// 밝게 만들기
        /// </summary>
        public void ConvertBrightnessColor()
        {
            var symbol = (this.Symbol as SimpleFillSymbol);
            if (symbol == null) return;

            this.origianlFillColor = (SolidColorBrush)symbol.Fill;
            this.originalBorderColor = (SolidColorBrush)symbol.BorderBrush;

            symbol.Fill = BrushUtil.SetSaturation(origianlFillColor, 0.7d);
            symbol.BorderBrush = BrushUtil.SetSaturation(originalBorderColor, 0.7d);
        }

        /// <summary>
        /// 
        /// </summary>
        public void RevertBrightnessColor()
        {
            var symbol = (this.Symbol as SimpleFillSymbol);
            if (symbol == null) return;

            symbol.Fill = origianlFillColor;
            symbol.BorderBrush = originalBorderColor;
        }

        /// <summary>
        /// 불투명하게 만들기
        /// </summary>
        public void ConvertOpaqueColor()
        {
            var symbol = (this.Symbol as SimpleFillSymbol);
            if (symbol == null) return;

            this.origianlFillColor = (SolidColorBrush)symbol.Fill;

            var opaqueColor = Color.FromArgb((byte)66, this.origianlFillColor.Color.R, this.origianlFillColor.Color.G,
                                             this.origianlFillColor.Color.B);

            symbol.Fill = new SolidColorBrush(opaqueColor);
            symbol.BorderBrush = originalBorderColor;
        }

        /// <summary>
        /// LinkZone Select 시 Border값 Black으로 설정 
        /// </summary>
        public void ToBorderBlack()
        {
            var symbol = (this.Symbol as SimpleFillSymbol);

            symbol.BorderBrush = new SolidColorBrush(Colors.Black);
            symbol.BorderThickness = 3;
            //symbol.BorderBrush = new 
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToBorderRed()
        {
            var symbol = (this.Symbol as SimpleFillSymbol);

            symbol.BorderBrush = new SolidColorBrush(Colors.Red);
            symbol.BorderThickness = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        public void ToBorderColor(Color color)
        {
            var symbol = (this.Symbol as SimpleFillSymbol);

            symbol.BorderBrush = new SolidColorBrush(color);
            symbol.BorderThickness = 3;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ToBorderOriginal()
        {
            if (this.isSelectedBySplunk)
            {
                this.ToBorderRed();
            }
            else
            {
                var symbol = (this.Symbol as SimpleFillSymbol);

                if (symbol != null)
                {
                    symbol.BorderBrush = new SolidColorBrush(this.borderColor);
                    symbol.BorderThickness = 1;
                }
            }

            this.RevertBrightnessColor();
        }

        /// <summary>
        /// Splunk 에서 색깔만 변경 해줄 경우
        /// </summary>
        /// <param name="color"></param>
        public void ChageColorOnlyBySplunk(object color)
        {
            this.isSelectedBySplunk = true;

            if (color == null)
            {
                this.isSelectedBySplunk = false;
                this.ChangeOriginalColor();
                return;
            }

            var symbol = (this.Symbol as SimpleFillSymbol);

            if (symbol == null) return;

            var newBrush = color is Brush ? (SolidColorBrush)color : new SolidColorBrush(new Color());

            var newColor = newBrush.Color;
            var alphaNewColor = Color.FromArgb(this.normalColor.A, (byte)newColor.R, (byte)newColor.G, (byte)newColor.B);

            if (!this.doNotChangeSymbolColor)
            {
                symbol.Fill = new SolidColorBrush(alphaNewColor);
                //this.ToBorderRed();
            }
            else
            {
                this.ToBorderColor(alphaNewColor);
            }

            this.origianlFillColor = (SolidColorBrush)symbol.Fill;
            this.originalBorderColor = (SolidColorBrush)symbol.BorderBrush;
        }

        #endregion Method

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {

        }

        #endregion Event Handler
    }
}
