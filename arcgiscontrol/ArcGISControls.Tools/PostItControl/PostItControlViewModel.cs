using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Interface;
using ArcGISControls.CommonData.Models;

namespace ArcGISControls.Tools.PostItControl
{
    public class PostItControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region Fields

        private string go;

        private string editUpdateSpl;

        private string editStatusSpl;

        private string app;

        private string ip;

        private string password;

        private string userId;

        private int port;

        #endregion

        #region Properties

        public string Go
        {
            get { return this.go; }
            set
            {
                this.go = value;
                this.OnPropertyChanged("Go");
            }
        }

        public string EditUpdateSpl
        {
            get { return this.editUpdateSpl; }
            set
            {
                this.editUpdateSpl = value;
                this.OnPropertyChanged("EditUpdateSpl");
            }
        }

        public string EditStatusSpl
        {
            get { return this.editStatusSpl; }
            set
            {
                this.editStatusSpl = value;
                this.OnPropertyChanged("EditStatusSpl");
            }
        }

        public string App
        {
            get { return this.app; }
            set
            {
                this.app = value;
                this.OnPropertyChanged("App");
            }
        }

        public string Ip
        {
            get { return this.ip; }
            set
            {
                this.ip = value;
                this.OnPropertyChanged("Ip");
            }
        }

        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                this.OnPropertyChanged("Password");
            }
        }

        public string UserId
        {
            get { return this.userId; }
            set
            {
                this.userId = value;
                this.OnPropertyChanged("UserId");
            }
        }

        public int Port
        {
            get { return this.port; }
            set
            {
                this.port = value;
                this.OnPropertyChanged("Port");
            }
        }

        #endregion

        #region Commands

        #endregion
    }
}
