using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ArcGISControls.CommonData.Models
{
    public class BaseMapTextObjectInfo : BaseMapObjectInfoData
    {
        private string text;

        [XmlElement("Text")]
        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                OnPropertyChanged("Text");
            }
        }

        private FontFamily fontFamily = new FontFamily("Malgun Gothic");

        [XmlIgnore]
        public FontFamily FontFamily
        {
            get { return this.fontFamily; }
            set
            {
                this.fontFamily = value;
                this.TextFont = this.fontFamily.Source;
                OnPropertyChanged("FontFamily");
            }
        }

        private string textFont;

        [XmlElement("TextFont")]
        public string TextFont
        {
            get { return this.textFont; }
            set
            {
                if( string.CompareOrdinal(this.textFont,value) == 0 ) return;

                this.textFont = value;
                OnPropertyChanged("TextFont");
            }
        }


        private bool? isBold = false;

        [XmlElement("IsBold")]
        public bool? IsBold
        {
            get { return this.isBold; }
            set
            {
                this.isBold = value;
                OnPropertyChanged("IsBold");
            }
        }

        private bool? isItalic = false;

        [XmlElement("IsItalic")]
        public bool? IsItalic
        {
            get { return this.isItalic; }
            set
            {
                this.isItalic = value;
                OnPropertyChanged("IsItalic");
            }
        }

        private bool? isUnderline = false;

        [XmlElement("IsUnderline")]
        public bool? IsUnderline
        {
            get { return this.isUnderline; }
            set
            {
                this.isUnderline = value;
                OnPropertyChanged("IsUnderline");
            }
        }

        private int? fontSize = 13;

        [XmlElement("FontSize")]
        public int? FontSize
        {
            get { return this.fontSize; }
            set
            {
                this.fontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        protected string fontColor;

        [XmlElement("FontColor")]
        public string FontColor
        {
            get { return this.fontColor; }
            set
            {
                this.fontColor = value;
                OnPropertyChanged("FontColor");
            }
        }

        protected string backgroundColor = string.Empty;

        [XmlElement("BackgroundColor")]
        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                OnPropertyChanged("BackgroundColor");
            }
        }

        private string borderColor;

        [XmlElement("BorderColor")]
        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                if (this.borderColor == value)
                    return;

                this.borderColor = value;
                this.OnPropertyChanged("BorderColor");
            }
        }

        private bool useBorder;

        [XmlIgnore]
        public bool UseBorder
        {
            get { return this.useBorder; }
            set
            {
                if(this.useBorder == value) return;

                this.useBorder = value;
                OnPropertyChanged("UseBorder");
            }
        }

        private TextAlignment textAlignment = TextAlignment.Left;

        [XmlElement("TextAlignment")]
        public TextAlignment TextAlignment
        {
            get { return this.textAlignment; }
            set
            {
                this.textAlignment = value;
                OnPropertyChanged("TextAlignment");
            }
        }

        private VerticalAlignment textVerticalAlignment;

        [XmlElement("TextVerticalAlignment")]
        public VerticalAlignment TextVerticalAlignment
        {
            get { return this.textVerticalAlignment; }
            set
            {
                this.textVerticalAlignment = value;
                OnPropertyChanged("TextVerticalAlignment");
            }
        }

        private Size textBoxSize;

        [XmlElement("TextBoxSize")]
        public Size TextBoxSize
        {
            get { return this.textBoxSize; }
            set
            {
                this.textBoxSize = value;
                OnPropertyChanged("TextBoxSize");
            }
        }


        public BaseMapTextObjectInfo()
            : base()
        {
            this.FontColor = "#FF000000";
        }

        public BaseMapTextObjectInfo(BaseMapTextObjectInfo data)
            : base(data)
        {
            this.Text = data.Text;
            this.TextFont = data.TextFont;
            this.FontFamily = new FontFamily(data.textFont);
            this.BackgroundColor = data.BackgroundColor;
            this.FontColor = data.FontColor;
            this.FontSize = data.FontSize;
            this.IsBold = data.IsBold;
            this.IsItalic = data.IsItalic;
            this.IsUnderline = data.IsUnderline;
            this.TextAlignment = data.TextAlignment;
            this.TextVerticalAlignment = data.TextVerticalAlignment;
            this.BorderColor = data.BorderColor;
            this.TextBoxSize = data.TextBoxSize;
        }
    }
}
