using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.GraphicObject;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.Helper
{
    /// <summary>
    /// Selected 될 Graphic 처리를 하도록 함 
    /// todo: ArcGISControl 의 selectedGraphicList를 이곳에서 사용하도록 한다.
    /// </summary>
    public class PropertyDataHelper
    {  
        public MapTextObjectDataInfo GetCommonData(MapTextObjectDataInfo dataInfo, List<BaseGraphic> selectedGraphics)
        {
            if (selectedGraphics.Count >= 1)
            {
                var textFont = dataInfo.TextFont;

                var cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.TextFont == textFont);

                dataInfo.TextFont = cnt == selectedGraphics.Count ? textFont : string.Empty;

                var isBold = dataInfo.IsBold;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.IsBold == isBold);

                dataInfo.IsBold = cnt == selectedGraphics.Count ? isBold : null;

                var isUnderLine = dataInfo.IsUnderline;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.IsUnderline == isUnderLine);

                dataInfo.IsUnderline = cnt == selectedGraphics.Count ? isUnderLine : null;

                var isItalic = dataInfo.IsItalic;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.IsItalic == isItalic);

                dataInfo.IsItalic = cnt == selectedGraphics.Count ? isItalic : null;

                var fontColor = dataInfo.FontColor;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.FontColor == fontColor);

                dataInfo.FontColorString = cnt == selectedGraphics.Count ? fontColor : null;

                var backgroundColor = dataInfo.BackgroundColor;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.BackgroundColor == backgroundColor);

                dataInfo.BackgroundColorString = cnt == selectedGraphics.Count ? backgroundColor : null;

                var fontSize = dataInfo.FontSize;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.FontSize == fontSize);

                dataInfo.FontSize = cnt == selectedGraphics.Count ? fontSize : null;

                var textAlign = dataInfo.TextAlignment;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.TextAlignment == textAlign);

                dataInfo.TextAlignment = cnt == selectedGraphics.Count ? textAlign : TextAlignment.Justify;

                var textVerticalAlign = dataInfo.TextVerticalAlignment;

                cnt = selectedGraphics.OfType<PolygonControlGraphic<TextBoxControl>>().Count(g => g.Control.DataInfo.TextVerticalAlignment == textVerticalAlign);

                dataInfo.TextVerticalAlignment = cnt == selectedGraphics.Count ? textVerticalAlign : ArcGISConstSet.TextObjectVerticalAlignment;
            }

            return dataInfo;//dataInfo.PropertyChanged += TextDataInfoOnPropertyChanged;

        }

        public MapLineObjectDataInfo GetCommonData(MapLineObjectDataInfo dataInfo, List<BaseGraphic> selectedGraphics)
        {
            if (selectedGraphics.Count >= 1)
            {
                var cnt = selectedGraphics.OfType<LineGraphic>().Count(g => g.DataInfo.ColorString == dataInfo.ColorString);

                dataInfo.ColorString = cnt == selectedGraphics.Count ? dataInfo.ColorString : null;

                cnt = selectedGraphics.OfType<LineGraphic>().Count(g => g.DataInfo.StrokeThickness == dataInfo.StrokeThickness);

                dataInfo.StrokeThickness = cnt == selectedGraphics.Count ? dataInfo.StrokeThickness : 0;

                cnt = selectedGraphics.OfType<LineGraphic>().Count(g => g.DataInfo.LineStrokeType == dataInfo.LineStrokeType);

                dataInfo.LineStrokeType = cnt == selectedGraphics.Count ? dataInfo.LineStrokeType : null;

                cnt = selectedGraphics.OfType<LineGraphic>().Count(g => g.DataInfo.LineJoin == dataInfo.LineJoin);

                dataInfo.LineJoin = cnt == selectedGraphics.Count ? dataInfo.LineJoin : null;
            }

            return dataInfo;
        }

        public CommonImageObjectDataInfo GetCommonData(CommonImageObjectDataInfo imageObjectData ,List<BaseGraphic> selectedGraphics)
        {   
            if(selectedGraphics.Count > 1)
            {
                var cnt = selectedGraphics.OfType<ImagePolygonGraphic>().Count(g => g.ImageObjectData.ImageDataStream == imageObjectData.ImageDataStream);

                imageObjectData.ImageDataStream = cnt == selectedGraphics.Count ? imageObjectData.ImageDataStream : null;

                cnt = selectedGraphics.OfType<ImagePolygonGraphic>().Count(g => g.ImageObjectData.ImageFileName == imageObjectData.ImageFileName);

                imageObjectData.ImageFileName = cnt == selectedGraphics.Count ? imageObjectData.ImageFileName : null;

                cnt = selectedGraphics.OfType<ImagePolygonGraphic>().Count(g => g.ImageObjectData.ImageOpacity == imageObjectData.ImageOpacity);

                imageObjectData.ImageOpacity = cnt == selectedGraphics.Count ? imageObjectData.ImageOpacity : 100;
            }

            return imageObjectData;
        }

        public void ChangeTextGraphic(string propertyName, MapTextObjectDataInfo newDataInfo, MapTextObjectDataInfo dataInfo, PolygonControlGraphic<TextBoxControl> graphic)
        {
            if (newDataInfo == null || dataInfo == null || graphic == null) return;

            if (propertyName.Equals("TextFont"))
            {
                dataInfo.TextFont = newDataInfo.TextFont;
                graphic.Control.DataInfo.TextFont = newDataInfo.TextFont;
            }

            if (propertyName.Equals("IsBold"))
            {
                dataInfo.IsBold = newDataInfo.IsBold;
                if (dataInfo.IsBold != null) graphic.Control.DataInfo.IsBold = newDataInfo.IsBold;
            }

            if (propertyName.Equals("IsItalic"))
            {
                dataInfo.IsItalic = newDataInfo.IsItalic;
                if (dataInfo.IsItalic != null) graphic.Control.DataInfo.IsItalic = newDataInfo.IsItalic.Value;
            }

            if (propertyName.Equals("IsUnderline"))
            {
                dataInfo.IsUnderline = newDataInfo.IsUnderline;
                if (dataInfo.IsUnderline != null) graphic.Control.DataInfo.IsUnderline = newDataInfo.IsUnderline.Value;
            }

            if (propertyName.Equals("FontSize"))
            {
                dataInfo.FontSize = newDataInfo.FontSize;
                if (dataInfo.FontSize != null) graphic.Control.DataInfo.FontSize = newDataInfo.FontSize.Value;
            }

            if (propertyName.Equals("FontColor"))
            {
                dataInfo.FontColor = newDataInfo.FontColor;
                if (dataInfo.FontColor != null) graphic.Control.DataInfo.FontColor = newDataInfo.FontColor;
            }

            if (propertyName.Equals("BackgroundColor"))
            {
                dataInfo.BackgroundColor = newDataInfo.BackgroundColor;
                if (dataInfo.BackgroundColor != null) graphic.Control.DataInfo.BackgroundColor = newDataInfo.BackgroundColor;
            }

            if (propertyName.Equals("TextAlignment"))
            {
                dataInfo.TextAlignment = newDataInfo.TextAlignment;
                if (dataInfo.TextAlignment != TextAlignment.Justify) graphic.Control.DataInfo.TextAlignment = newDataInfo.TextAlignment;
            }

            if (propertyName.Equals("TextVerticalAlignment"))
            {
                dataInfo.TextVerticalAlignment = newDataInfo.TextVerticalAlignment;
                graphic.Control.DataInfo.TextVerticalAlignment = newDataInfo.TextVerticalAlignment;
            }
        }

        public void ChangeGraphic(string propertyName, MapLineObjectDataInfo newDataInfo, LineGraphic graphic)
        {
            if (newDataInfo == null || graphic == null) return;

            if(propertyName.Equals("Name"))
            {   
                if (string.IsNullOrEmpty(newDataInfo.Name)) return;

                graphic.DataInfo.Name = newDataInfo.Name;
            }
            else if (propertyName.Equals("ColorString"))
            {
                var convertFromString = ColorConverter.ConvertFromString(newDataInfo.ColorString);
                if (convertFromString != null)
                    graphic.LineColor = (Color)convertFromString;
            }
            else if (propertyName.Equals("StrokeThickness"))
            {
                graphic.StrokeThickness = newDataInfo.StrokeThickness;
            }
            else if (propertyName.Equals("LineStrokeType"))
            {
                graphic.LineStrokeType = newDataInfo.LineStrokeType != null ? newDataInfo.LineStrokeType.Value : ArcGISConstSet.LineStrokeType;
            }
            else if (propertyName.Equals("LineJoin"))
            {
                graphic.LineJoin = newDataInfo.LineJoin != null ? newDataInfo.LineJoin.Value : ArcGISConstSet.LineJoin;
            }
            else if (propertyName.Equals("StartLineCap"))
            {
                graphic.StartLineCap = newDataInfo.StartLineCap;
            }
            else if (propertyName.Equals("EndLineCap"))
            {
                graphic.EndLineCap = newDataInfo.EndLineCap;
            }
        }

        public void ChangeGraphic(string propertyName, MapImageObjectDataInfo newDataInfo, MapImageObjectDataInfo dataInfo, ImagePolygonGraphic graphic)
        {
            if (newDataInfo == null || graphic == null) return;

            if (propertyName.Equals("Name"))
            {
                if (string.IsNullOrEmpty(newDataInfo.Name)) return;

                dataInfo.Name = newDataInfo.Name;
            }

            if (propertyName.ToLower().Equals("imageobjectdata"))
            {
                if (newDataInfo.ImageObjectData.ImageDataStream != graphic.ImageObjectData.ImageDataStream)
                {
                    graphic.ChageSymbolImage(newDataInfo.ImageObjectData.ImageDataStream);
                }

                if (newDataInfo.ImageObjectData.ImageFileName != graphic.ImageObjectData.ImageFileName)
                {
                    graphic.ImageObjectData.ImageFileName = newDataInfo.ImageObjectData.ImageFileName;
                }

                if (newDataInfo.ImageObjectData.ImageOpacity != graphic.ImageObjectData.ImageOpacity)
                {
                    graphic.ChangeSymbolImageOpacity(newDataInfo.ImageObjectData.ImageOpacity);
                }
            }
        }

        /// <summary>
        /// Common ImageData수정
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="newDataInfo"></param>
        /// <param name="graphic"></param>
        public void ChangeGraphic(string propertyName, CommonImageObjectDataInfo newDataInfo, ImagePolygonGraphic graphic)
        {
            if (propertyName.Equals("ImageDataStream"))
            {
                graphic.ChageSymbolImage(newDataInfo.ImageDataStream, newDataInfo.ImageOpacity);
            }

            if (propertyName.Equals("ImageFileName"))
            {
                graphic.ImageObjectData.ImageFileName = newDataInfo.ImageFileName;
            }

            if (propertyName.Equals("ImageOpacity"))
            {
                graphic.ChangeSymbolImageOpacity(newDataInfo.ImageOpacity);
            }
        }
    }
}
