using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.GraphicObject
{
    public class TextBoxControlGraphic : PolygonControlGraphic<TextBoxControl>
    {
        public TextBoxControlGraphic(string id, List<Point> pointCollection)
            : base(new TextBoxControl(), MapObjectType.MemoTextBox, id, pointCollection)
        {
        }

        /// <summary>
        /// Text Box 일 경우 Text 로 보낸다 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pointCollection"></param>
        /// <param name="textBox"></param>
        public TextBoxControlGraphic(string id, List<Point> pointCollection, TextBoxControl textBox)
            : base(textBox, MapObjectType.Text, id, pointCollection)
        {
        }
    }
}
