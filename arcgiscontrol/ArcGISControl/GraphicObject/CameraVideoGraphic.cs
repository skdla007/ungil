using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ArcGISControl.UIControl.GraphicObjectControl;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using PointCollection = ESRI.ArcGIS.Client.Geometry.PointCollection;

namespace ArcGISControl.GraphicObject
{
    public class CameraVideoGraphic : PolygonControlGraphic<CameraVideoControl>
    {
        #region Field

        /// <summary>
        /// Camera DB 값
        /// </summary>
        private readonly string cameraInformationId = string.Empty;
        public string CameraInformationID
        {
            get { return cameraInformationId; }
        }

        /// <summary>
        /// VW에서 권한에 대한 정보
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// VIDE SHOW 정보
        /// </summary>
        public bool ShowVideo { get; set; }

        /// <summary>
        /// 현재 보이고 있는 맵 영역에 카메라가 안보여도 영상을 계속 유지 할 것인지 여부
        /// </summary>
        public bool AlwaysKeepToCameraVideo { get; set; }

        #endregion // Field

        #region Construction

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pointCollection"></param>
        /// <param name="type"></param>
        /// <param name="id">UnigridID</param>
        /// <param name="cameraRealId"></param>
        public CameraVideoGraphic(List<Point> pointCollection, string id, string cameraInformationID)
            : base(new CameraVideoControl(), MapObjectType.CameraVideo, id, pointCollection)
        {
            this.cameraInformationId = cameraInformationID;
            this.ShowVideo = true;
        }

        #endregion // Construcstion

        #region Method

        private void ShowSelectionBorder()
        {
            this.Control.SelectionBorderVisibility = Visibility.Visible;
        }

        private void HideSelectionBorder()
        {
            this.Control.SelectionBorderVisibility = Visibility.Collapsed;
        }

        #endregion // Method

        #region Event Handler

        protected override void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs propertyChangedEventArgs)
        {
            base.OnPropertyChanged(sender, propertyChangedEventArgs);

            if (propertyChangedEventArgs.PropertyName == "Selected")
            {
                var g = sender as BaseGraphic;

                if (g != null)
                {
                    if(g.SelectFlag)
                    {
                        this.ShowSelectionBorder();
                    }
                    else
                    {
                        this.HideSelectionBorder();
                    }
                }
            }
        }

        #endregion Event Handler
    }
}
