using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.PropertyControl
{
    public class CommonPropertyWindowViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        private CameraIconPropertyControlViewModel cameraIconPropertyControlViewModel;

        public double MapLevel 
        {
            set
            {
                var mapLevel = (int)Math.Round(value);
                if (this.cameraVideoPropertyControlViewModel != null)
                {
                    this.cameraVideoPropertyControlViewModel.MapLevel = mapLevel;
                }
                if (this.splunkPropertyControlViewModel != null)
                {
                    this.splunkPropertyControlViewModel.MapLevel = mapLevel;
                }
            }
        }

        public double MapResolution
        {
            set
            {
                if (this.cameraVideoPropertyControlViewModel != null)
                {   
                    this.cameraVideoPropertyControlViewModel.MapCurrentResoultion = value;
                }
            }
        }

        public CameraIconPropertyControlViewModel CameraIconPropertyControlViewModel
        {
            get { return this.cameraIconPropertyControlViewModel; }
            set
            {
                if (this.cameraIconPropertyControlViewModel == value)
                    return;
                this.cameraIconPropertyControlViewModel = value;
                this.OnPropertyChanged("CameraIconPropertyControlViewModel");
            }
        }

        private CameraVideoPropertyControlViewModel cameraVideoPropertyControlViewModel;

        public CameraVideoPropertyControlViewModel CameraVideoPropertyControlViewModel
        {
            get { return this.cameraVideoPropertyControlViewModel; }
            set
            {
                if (this.cameraVideoPropertyControlViewModel == value)
                    return;
                this.cameraVideoPropertyControlViewModel = value;
                this.OnPropertyChanged("CameraVideoPropertyControlViewModel");
            }
        }

        private CameraPresetPropertyControlViewModel cameraPresetPropertyControlViewModel;

        public CameraPresetPropertyControlViewModel CameraPresetPropertyControlViewModel
        {
            get { return this.cameraPresetPropertyControlViewModel; }
            set
            {
                if (this.cameraPresetPropertyControlViewModel == value)
                    return;
                this.cameraPresetPropertyControlViewModel = value;
                this.OnPropertyChanged("CameraPresetPropertyControlViewModel");
            }
        }

        private LinkZonePropertyControlViewModel linkZonePropertyControlViewModel;

        public LinkZonePropertyControlViewModel LinkZonePropertyControlViewModel
        {
            get { return this.linkZonePropertyControlViewModel; }
            set
            {
                if (this.linkZonePropertyControlViewModel == value)
                    return;
                this.linkZonePropertyControlViewModel = value;
                this.OnPropertyChanged("LinkZonePropertyControlViewModel");
            }
        }

        private PlacePropertyControlViewModel placePropertyControlViewModel;

        public PlacePropertyControlViewModel PlacePropertyControlViewModel
        {
            get { return this.placePropertyControlViewModel; }
            set
            {
                if (this.placePropertyControlViewModel == value)
                    return;
                this.placePropertyControlViewModel = value;
                this.OnPropertyChanged("PlacePropertyControlViewModel");
            }
        }

        private SplunkPropertyControlViewModel splunkPropertyControlViewModel;

        public SplunkPropertyControlViewModel SplunkPropertyControlViewModel
        {
            get { return this.splunkPropertyControlViewModel; }
            set
            {
                if (this.splunkPropertyControlViewModel == value)
                    return;
                this.splunkPropertyControlViewModel = value;
                this.OnPropertyChanged("SplunkPropertyControlViewModel");
            }
        }

        private WorkStationPropertyControlViewModel workStationPropertyControlViewModel;
        
        public WorkStationPropertyControlViewModel WorkStationPropertyControlViewModel
        {
            get { return this.workStationPropertyControlViewModel; }
            set
            {
                if(this.workStationPropertyControlViewModel == value)
                    return;

                this.workStationPropertyControlViewModel = value;
                this.OnPropertyChanged("WorkStationPropertyControlViewModel");
            }
        }

        private TextPropertyControlViewModel textPropertyControlViewModel;

        public TextPropertyControlViewModel TextPropertyControlViewModel
        {
            get { return this.textPropertyControlViewModel; }
            set
            {
                if(this.textPropertyControlViewModel == value)
                    return;

                this.textPropertyControlViewModel = value;
                this.OnPropertyChanged("TextPropertyControlViewModel");
            }
        }

        private LinePropertyControlViewModel linePropertyControlViewModel;

        public LinePropertyControlViewModel LinePropertyControlViewModel
        {
            get { return this.linePropertyControlViewModel; }
            set
            {
                if(this.linePropertyControlViewModel == value)
                    return;

                this.linePropertyControlViewModel = value;
                this.OnPropertyChanged("LinePropertyControlViewModel");
            }
        }

        private ImagePropertyControlViewModel imagePropertyControlViewModel;

        public ImagePropertyControlViewModel ImagePropertyControlViewModel
        {
            get { return this.imagePropertyControlViewModel; }
            set
            {
                if(this.imagePropertyControlViewModel == value) return;

                this.imagePropertyControlViewModel = value;
                this.OnPropertyChanged("ImagePropertyControlViewModel");
            }
        }

        private UniversalObjectPropertyControlViewModel universalObjectPropertyControlViewModel;

        public UniversalObjectPropertyControlViewModel UniversalObjectPropertyControlViewModel
        {
            get { return this.universalObjectPropertyControlViewModel; }
            set
            {
                if (this.universalObjectPropertyControlViewModel == value) return;

                this.universalObjectPropertyControlViewModel = value;
                this.OnPropertyChanged("UniversalObjectPropertyControlViewModel");
            }
        }

        private MapObjectPropertied[] _MapObjectProperties = { };
        public MapObjectPropertied[] MapObjectPropertiedVisible
        {
            get
            {
                return _MapObjectProperties;
            }
            set
            {
                _MapObjectProperties = value;
                this.OnPropertyChanged("MapObjectPropertiedVisible");
            }
        }

        private bool cameraIconSelected;

        public bool CameraIconSelected
        {
            get { return this.cameraIconSelected; }
            set
            {
                if (this.cameraIconSelected == value)
                    return;
                this.cameraIconSelected = value;
                this.OnPropertyChanged("CameraIconSelected");
            }
        }

        private bool cameraVideoSelected;

        public bool CameraVideoSelected
        {
            get { return this.cameraVideoSelected; }
            set
            {
                if (this.cameraVideoSelected == value)
                    return;
                this.cameraVideoSelected = value;
                this.OnPropertyChanged("CameraVideoSelected");
            }
        }

        private bool cameraViewZoneSelected;

        public bool CameraViewZoneSelected
        {
            get { return this.cameraViewZoneSelected; }
            set
            {
                if (this.cameraViewZoneSelected == value)
                    return;
                this.cameraViewZoneSelected = value;
                this.OnPropertyChanged("CameraViewZoneSelected");
            }
        }

        private bool linkZoneSelected;

        public bool LinkZoneSelected
        {
            get { return this.linkZoneSelected; }
            set
            {
                if (this.linkZoneSelected == value)
                    return;
                this.linkZoneSelected = value;
                this.OnPropertyChanged("LinkZoneSelected");
            }
        }

        private bool placeSelected;

        public bool PlaceSelected
        {
            get { return this.placeSelected; }
            set
            {
                if (this.placeSelected == value)
                    return;
                this.placeSelected = value;
                this.OnPropertyChanged("PlaceSelected");
            }
        }

        private bool splunkSelected;

        public bool SplunkSelected
        {
            get { return this.splunkSelected; }
            set
            {
                if (this.splunkSelected == value)
                    return;
                this.splunkSelected = value;
                this.OnPropertyChanged("SplunkSelected");
            }
        }

        private bool workStationSelected;

        public bool WorkStationSelected
        {
            get { return this.workStationSelected; }
            set
            {
                if (this.workStationSelected == value)
                    return;
                this.workStationSelected = value;
                this.OnPropertyChanged("WorkStationSelected");
            }
        }

        private bool textSelected;

        public bool TextSelected
        {
            get { return this.textSelected; }
            set
            {
                if (this.textSelected == value)
                    return;
                this.textSelected = value;
                this.OnPropertyChanged("TextSelected");
            }
        }

        private bool lineSelected;

        public bool LineSelected
        {
            get { return this.lineSelected; }
            set
            {
                if(this.lineSelected == value) return;

                this.lineSelected = value;
                this.OnPropertyChanged("LineSelected");
            }
        }

        private bool imageSelected;

        public bool ImageSelected
        {
            get { return this.imageSelected; }
            set
            {
                if(this.imageSelected == value) return;

                this.imageSelected = value;
                this.OnPropertyChanged("ImageSelected");
            }
        }

        private bool universalObjectSelected;

        public bool UniversalObjectSelected
        {
            get { return this.universalObjectSelected; }
            set
            {
                if (this.universalObjectSelected == value) return;

                this.universalObjectSelected = value;
                this.OnPropertyChanged("UniversalObjectSelected");
            }
        }
    }
}
