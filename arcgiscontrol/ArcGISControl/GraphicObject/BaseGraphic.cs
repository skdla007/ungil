using System;
using System.ComponentModel;
using System.Windows;
using ArcGISControls.CommonData.Types;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;

namespace ArcGISControl.GraphicObject
{
    public class BaseGraphic : Graphic
    {

        private bool selectFlag;
        public bool SelectFlag
        { 
            get { return this.selectFlag; }
            set
            {

                //this.Select();

                this.selectFlag = value;

                if (this.selectFlag)
                {
                    this.Select();
                }
                else
                {
                    this.UnSelect();
                }
            }
        }

        public Point MaxPosition
        {
            get
            {
                return new Point(this.Geometry.Extent.XMax, this.Geometry.Extent.YMax);
            }
        }

        public Point MinPosition
        {
            get
            {
                return new Point(this.Geometry.Extent.XMin, this.Geometry.Extent.YMin);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string objectId;
        public string ObjectID
        {
            get { return this.objectId; }
            set
            {
                this.objectId = value;
                this.Attributes["ID"] = objectId;
            }
        }

        /// <summary>
        /// Redo/Undo의 CommandTransform 명령에서 사용된다.
        /// Camera Graphic같은 경우 Graphic을 Undo(삭제) 후 Redo(다시생성)할때마다 ObjectID가 변경되므로, 이전 ObjectID를 알고 있어야 한다.
        /// </summary>
        public string FirstObjectID
        {
            get;
            set;
        }

        private bool _LockFlag;
        public bool IsLocked
        {
            get { return _LockFlag; }
            set
            {
                _LockFlag = value;
            }
        }

        private MapObjectType type;
        public MapObjectType Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                this.Attributes["Type"] = type;
            }
        }

        public int ZIndex
        {
            get { return this.GetZIndex(); }
            set
            {
                this.SetZIndex(value);
                this.RaiseZIndexChangedEvent();
            }
        }

        public event EventHandler ZIndexChanged;
        private UIControl.GraphicObjectControl.LineCanvasControl lineCanvas;
        private MapObjectType mapObjectType;
        private string id;
        private System.Collections.Generic.List<Point> pointCollection;

        private void RaiseZIndexChangedEvent()
        {
            var e = this.ZIndexChanged;
            if (e != null)
            {
                e(this, EventArgs.Empty);
            }
        }

        public ArcGISControls.CommonData.Models.BaseMapObjectInfoData ObjectInfoData { get; set; }

        #region Construction

        public BaseGraphic(MapObjectType type, string id)
        {
            this.Type = type;
            this.ObjectID = id;

            this.PropertyChanged += OnPropertyChanged;
        }

        public BaseGraphic(UIControl.GraphicObjectControl.LineCanvasControl lineCanvas, MapObjectType mapObjectType, string id, System.Collections.Generic.List<Point> pointCollection)
        {
            // TODO: Complete member initialization
            this.lineCanvas = lineCanvas;
            this.mapObjectType = mapObjectType;
            this.id = id;
            this.pointCollection = pointCollection;
        }

        #endregion 

        #region EventHandler

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            
        }

        #endregion 

        #region Method
        
        #endregion //Method
    }
}
