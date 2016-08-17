using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ArcGISControl.Bases;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class IconGraphic : BaseGraphic, IPositionOwner
    {
        #region Field

        /// <summary>
        /// Geometry가 MapPoint가 아니거나 null일 때 참조하면 InvalidOperationException을 던집니다.
        /// </summary>
        public Point Position
        {
            get
            {
                var mapPoint = this.Geometry as MapPoint;
                if (mapPoint == null)
                    throw new InvalidOperationException("Geometry가 MapPoint가 아니거나 null입니다.");

                return new Point(mapPoint.X, mapPoint.Y);
            }

            set
            {
                this.Geometry = new MapPoint(value.X, value.Y);
                this.RaisePositionChangedEvent();
            }
        }

        public event EventHandler<EventArgs> PositionChanged;

        private void RaisePositionChangedEvent()
        {
            if (this.PositionChanged != null)
            {
                this.PositionChanged(this, EventArgs.Empty);
            }
        }

        private Size imagePixelSize = Size.Empty;
        public Size ImagePixelSize
        {
            get { return this.imagePixelSize; }
        }

        private Point setOffset;
        public Point OffsetPoint
        {
            get
            {
                if (this.Symbol == null || (this.Symbol as PictureMarkerSymbol)  == null)
                    return new Point(0, 0);
                else
                    return new Point((this.Symbol as PictureMarkerSymbol).OffsetX, (this.Symbol as PictureMarkerSymbol).OffsetY);
            }
        }

        //Noraml상황일때 ImageUrl
        protected string iconNormalUriString = string.Empty;
        public string IconNormalUriString
        {
            get { return this.iconNormalUriString; }
            set
            {
                this.iconNormalUriString = value;
            }
        }

        //Over 상황일때 ImageUrl
        protected string iconSelectedUriString = string.Empty;
        public string IconSelectedrUriString
        {
            get { return this.iconSelectedUriString; }
            set
            {
                this.iconSelectedUriString = value;
            }
        }

        private double iconSize = 1;
        public double IconSize
        {
            get { return this.iconSize; }
        }

       #endregion // Field

        #region Construction 

        /// <summary>
        /// Map에 Icon Graphic을 만든다
        /// </summary>
        /// <param name="position"></param>
        /// <param name="iconUrl"></param>
        /// <param name="iconSelectedUrl"></param>
        /// <param name="type">Object type</param>
        /// <param name="id">UnigridID</param>
        /// <param name="offsetPoint">UnigridID</param>
        public IconGraphic(Point position, string iconUrl, string iconSelectedUrl, 
            MapObjectType type, string id, Point offsetPoint)
            : base(type, id)
        {
            this.setOffset = offsetPoint;
            this.Position = position;
            this.Symbol = new PictureMarkerSymbol();

            this.IconNormalUriString = iconUrl;
            this.IconSelectedrUriString = iconSelectedUrl;

            this.ChangeSymbolImage(iconUrl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="iconUrl"></param>
        /// <param name="iconSelectedUrl"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="size"></param>
        public IconGraphic(Point position, string iconUrl, string iconSelectedUrl, MapObjectType type, string id, double size = 1)
            : base(type, id)
        {
            this.setOffset = new Point(0, 0);
            this.Position = position;
            this.Symbol = new PictureMarkerSymbol();

            this.IconNormalUriString = iconUrl;
            this.IconSelectedrUriString = iconSelectedUrl;

            iconSize = size;

            this.ChangeSymbolImage(iconUrl);
        }

        #endregion // Construcstion 

        #region Method

        /// <summary>
        /// Graphic 선택시 심볼 이미지 바꾸기
        /// </summary>
        /// <param name="imageUrl"></param>
        public void ChangeSymbolImage(string imageUrl)
        {
            if (this.iconSize <= 0) this.iconSize = 1;

            var pictureMarkerSymbol = this.Symbol as PictureMarkerSymbol;

            if (pictureMarkerSymbol == null) return;

            var bitmapImage = pictureMarkerSymbol.Source as BitmapImage ?? new BitmapImage();

            if ((bitmapImage.UriSource != null && bitmapImage.UriSource.ToString() == imageUrl) || imageUrl == null)
            {
                return;
            }

            bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(imageUrl, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();

            pictureMarkerSymbol.Source = bitmapImage;

            pictureMarkerSymbol.Width = bitmapImage.PixelWidth * this.iconSize;
            pictureMarkerSymbol.Height = bitmapImage.PixelHeight * this.iconSize;

            if (this.setOffset.X == 0 && this.setOffset.Y == 0)
            {
                pictureMarkerSymbol.OffsetX = (pictureMarkerSymbol.Width / 2);
                pictureMarkerSymbol.OffsetY = pictureMarkerSymbol.Height;
            }
            else
            {
                pictureMarkerSymbol.OffsetX = this.setOffset.X;
                pictureMarkerSymbol.OffsetY = this.setOffset.Y;
            }

            this.imagePixelSize = new Size(pictureMarkerSymbol.Width, pictureMarkerSymbol.Height);

            this.Symbol = pictureMarkerSymbol;
        }

        public void ChangeIconSize(double size)
        {
            this.iconSize = size;
            if (this.iconSize <= 0) this.iconSize = 1;

            var pictureMarkerSymbol = this.Symbol as PictureMarkerSymbol;

            if (pictureMarkerSymbol == null) return;

            var bitmapImage = pictureMarkerSymbol.Source as BitmapImage ?? new BitmapImage();

            if (bitmapImage.UriSource == null || iconNormalUriString == null)
            {
                return;
            }


            pictureMarkerSymbol.Width = bitmapImage.PixelWidth * this.iconSize;
            pictureMarkerSymbol.Height = bitmapImage.PixelHeight * this.iconSize;

            pictureMarkerSymbol.OffsetX = (pictureMarkerSymbol.Width / 2);
            pictureMarkerSymbol.OffsetY = pictureMarkerSymbol.Height;

            this.imagePixelSize = new Size(pictureMarkerSymbol.Width, pictureMarkerSymbol.Height);
        }

        public void ChangeOffsetPoint(Point point)
        {
            this.setOffset = point;

            if (this.iconSize <= 0) this.iconSize = 1;

            var pictureMarkerSymbol = this.Symbol as PictureMarkerSymbol;

            if (pictureMarkerSymbol == null) return;

            var bitmapImage = pictureMarkerSymbol.Source as BitmapImage ?? new BitmapImage();

            if (bitmapImage.UriSource == null || iconNormalUriString == null)
            {
                return;
            }

            pictureMarkerSymbol.OffsetX = setOffset.X;
            pictureMarkerSymbol.OffsetY = setOffset.Y;
        }
        
        #endregion //Method

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            base.OnPropertyChanged(sender, propertyChangedEventArgs);

            if (propertyChangedEventArgs.PropertyName == "Selected")
            {
                var g = sender as IconGraphic;
                
                if(g != null )
                {
                    this.ChangeSymbolImage(g.Selected ? this.iconSelectedUriString : this.iconNormalUriString);    
                }
            }
        }

        #endregion Event Handler

    }
}
