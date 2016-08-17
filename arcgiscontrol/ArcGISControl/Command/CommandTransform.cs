using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System;
using System.Linq;
using System.Collections.Generic;
using ArcGISControl.GraphicObject;
using System.Windows;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Types;

namespace ArcGISControl.Command
{
    /// <summary>
    /// Graphic transform Command
    /// </summary>
    class CommandTransform : CommandBase
    {
        private List<BaseGraphic> _TransformObjectCloneList = new List<BaseGraphic>();
        private List<Geometry> _OriginGeometryCloneList = new List<Geometry>();
        private List<Geometry> _ModifiedGeometryCloneList = new List<Geometry>();
        private Action<BaseGraphic, List<Point>, bool> _SetObjectPositionAction;
        internal delegate void SetSpecialObjectMove(BaseGraphic graphic, Vector displacement, bool CameraGraphicMoveLoop = false);
        private SetSpecialObjectMove _SetSpecialObjectMoveMethod;

        #region Constructor
        public CommandTransform(Action<BaseGraphic, List<Point>, bool> SetObjectPositionAction, SetSpecialObjectMove SetSpecialObjectMoveMethod)
            : base()
        {
            _SetObjectPositionAction = SetObjectPositionAction;
            _SetSpecialObjectMoveMethod = SetSpecialObjectMoveMethod;
        }
        #endregion

        #region Properties
        public int ModifiedGeometryCount
        {
            get { return _ModifiedGeometryCloneList.Count; }
        }
        #endregion

        #region CommandBase 구현
        public void AddTransformObjectClone(BaseGraphic TransformObjectClone, Geometry OriginGeometryClone, Geometry ModifiedGeometryClone)
        {
            // Keep copy of Transform object
            _TransformObjectCloneList.Add(TransformObjectClone);
            _OriginGeometryCloneList.Add(OriginGeometryClone);
            _ModifiedGeometryCloneList.Add(ModifiedGeometryClone);
        }

        public void RemoveTransformObjectClone(BaseGraphic baseGraphic)
        {
            if (_TransformObjectCloneList != null)
            {
                int Idx = _TransformObjectCloneList.IndexOf(baseGraphic);
                _OriginGeometryCloneList.RemoveAt(Idx);
                _ModifiedGeometryCloneList.RemoveAt(Idx);
                _TransformObjectCloneList.RemoveAt(Idx);
            }
        }

        public override void Undo(GraphicsLayer ObjectGraphicLayer)
        {
            Editor.Stop();
            if (_OriginGeometryCloneList != null && _OriginGeometryCloneList.Count > 0)
            {
                for (int i = 0; i < _TransformObjectCloneList.Count; i++)
                {
                    // Redo / Undo로 인해 Graphic이 삭제 후 다시 생성된 경우
                    if (ObjectGraphicLayer.Graphics.Contains(_TransformObjectCloneList[i]) == false)
                    {
                        BaseGraphic CreatedGraphic = null;
                        if (_TransformObjectCloneList[i].Type == MapObjectType.CameraPreset)
                        {
                            //CameraPresetGraphic
                            CreatedGraphic = (BaseGraphic)ObjectGraphicLayer.Graphics.FirstOrDefault(p =>
                            {
                                return ((BaseGraphic)p).Type == MapObjectType.CameraPreset && ((CameraPresetGraphic)p).FirstObjectID != null && ((CameraPresetGraphic)p).FirstObjectID.Equals(_TransformObjectCloneList[i].ObjectID) &&
                                    ((CameraPresetGraphic)p).PresetIndex == ((CameraPresetGraphic)_TransformObjectCloneList[i]).PresetIndex;
                            });
                        }
                        else
                        {
                            CreatedGraphic = (BaseGraphic)ObjectGraphicLayer.Graphics.FirstOrDefault(p =>
                            (
                                (((BaseGraphic)p).ObjectID.Equals(_TransformObjectCloneList[i].ObjectID) || (((BaseGraphic)p).FirstObjectID != null &&  ((BaseGraphic)p).FirstObjectID.Equals(_TransformObjectCloneList[i].ObjectID)))
                            ) && ((BaseGraphic)p).Type == _TransformObjectCloneList[i].Type);
                        }

                        if (CreatedGraphic == null) return;
                        _TransformObjectCloneList[i] = CreatedGraphic;
                    }

                    UnSelectGraphicObjectMethod(_TransformObjectCloneList[i]);
                    _TransformObjectCloneList[i].UnSelect();
                    this.SetObjectPosition(_TransformObjectCloneList[i], _OriginGeometryCloneList[i]);
                    //_TransformObjectCloneList[i].Geometry = _OriginGeometryCloneList[i];
                }
            }
        }

        public override void Redo(GraphicsLayer ObjectGraphicLayer)
        {
            Editor.Stop();
            if (_ModifiedGeometryCloneList != null && _ModifiedGeometryCloneList.Count > 0)
            {
                for (int i = 0; i < _TransformObjectCloneList.Count; i++)
                {
                    // Redo / Undo로 인해 Graphic이 삭제 후 다시 생성된 경우
                    if (ObjectGraphicLayer.Graphics.Contains(_TransformObjectCloneList[i]) == false)
                    {
                        BaseGraphic CreatedGraphic = null;
                        if (_TransformObjectCloneList[i].Type == MapObjectType.CameraPreset)
                        {
                            //CameraPresetGraphic
                            CreatedGraphic = (BaseGraphic)ObjectGraphicLayer.Graphics.FirstOrDefault(p =>
                            {
                                return ((BaseGraphic)p).Type == MapObjectType.CameraPreset && ((CameraPresetGraphic)p).FirstObjectID != null && ((CameraPresetGraphic)p).FirstObjectID.Equals(_TransformObjectCloneList[i].ObjectID) &&
                                    ((CameraPresetGraphic)p).PresetIndex == ((CameraPresetGraphic)_TransformObjectCloneList[i]).PresetIndex;
                            });
                        }
                        else
                        {
                            CreatedGraphic = (BaseGraphic)ObjectGraphicLayer.Graphics.FirstOrDefault(p =>
                            {
                                return (((BaseGraphic)p).ObjectID.Equals(_TransformObjectCloneList[i].ObjectID) || (((BaseGraphic)p).FirstObjectID != null && ((BaseGraphic)p).FirstObjectID.Equals(_TransformObjectCloneList[i].ObjectID)))
                                    && ((BaseGraphic)p).Type == _TransformObjectCloneList[i].Type;
                            });
                        }
                        
                        if (CreatedGraphic == null) return;
                        _TransformObjectCloneList[i] = CreatedGraphic;
                    }

                    UnSelectGraphicObjectMethod(_TransformObjectCloneList[i]);
                    _TransformObjectCloneList[i].UnSelect();
                    this.SetObjectPosition(_TransformObjectCloneList[i], _ModifiedGeometryCloneList[i]);
                    //_TransformObjectCloneList[i].Geometry = _ModifiedGeometryCloneList[i];
                }
            }
        }
        #endregion

        private void SetObjectPosition(BaseGraphic baseGraphic, Geometry ModifiedGeometry)
        {
            if(this.IsOnlyMoveGraphcType(baseGraphic))
            {
                Vector Displacement = new Vector((((MapPoint)ModifiedGeometry).X - ((MapPoint)baseGraphic.Geometry).X), (((MapPoint)ModifiedGeometry).Y - ((MapPoint)baseGraphic.Geometry).Y));
                _SetSpecialObjectMoveMethod(baseGraphic, Displacement);
                return;
            }

            bool ChangeControlSize = false;
            List<Point> newPoints = null;

            if (baseGraphic is IPointCollectionOwner)
            {
                List<Point> oldPoints = (baseGraphic as IPointCollectionOwner).PointCollection;
                List<MapPoint> newMapPoints = this.GetGraphicPoints(ModifiedGeometry);
                newPoints = newMapPoints.Select(p => new Point(p.X, p.Y)).ToList();

                if (NumberUtil.AreSame(oldPoints.Max(e => e.X) - oldPoints.Min(e => e.X), newPoints.Max(e => e.X) - newPoints.Min(e => e.X)) &&
                    NumberUtil.AreSame(oldPoints.Max(e => e.Y) - oldPoints.Min(e => e.Y), newPoints.Max(e => e.Y) - newPoints.Min(e => e.Y))) ChangeControlSize = false;

                (baseGraphic as IPointCollectionOwner).PointCollection = newPoints;
            }

            _SetObjectPositionAction(baseGraphic, newPoints, ChangeControlSize);
        }

        private bool IsOnlyMoveGraphcType(BaseGraphic baseGraphic)
        {
            return (baseGraphic.Type == MapObjectType.CameraNameTextBox || baseGraphic.Type == MapObjectType.CameraPresetPlus ||
                baseGraphic.Type == MapObjectType.CameraIcon || baseGraphic.Type == MapObjectType.Location ||
                baseGraphic.Type == MapObjectType.Address || baseGraphic.Type == MapObjectType.SearchedAddress);
        }

        private List<MapPoint> GetGraphicPoints(Geometry graphicGeometry)
        {
            List<MapPoint> points = new List<MapPoint>();
            if (graphicGeometry is Polygon)
            {
                points = (graphicGeometry as Polygon).Rings.ElementAt(0).ToList();
            }
            else if (graphicGeometry is Polyline)
            {
                points = (graphicGeometry as Polyline).Paths.ElementAt(0).ToList();
            }
            return points;
        }

        private List<MapPoint> GetMoveGraphicPointsByVector(BaseGraphic baseGraphic, Vector displacement)
        {
            List<MapPoint> points = this.GetGraphicPoints(baseGraphic.Geometry);
            List<MapPoint> movePointCollection = new List<MapPoint>();
            points.ForEach(p => movePointCollection.Add(new MapPoint(p.X + displacement.X, p.Y + displacement.Y)));

            return movePointCollection;
        }
    }
}
