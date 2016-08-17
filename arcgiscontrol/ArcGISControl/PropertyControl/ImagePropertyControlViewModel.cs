using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGISControl.Helper;
using ArcGISControls.CommonData.Models;
using ArcGISControls.CommonData.Types;
using Microsoft.Win32;

namespace ArcGISControl.PropertyControl
{
    public class ImagePropertyControlViewModel : DataChangedNotify.NotifyPropertyChanged
    {
        #region fields

        private bool isSingleSetting = true;

        public bool IsSingleSetting
        {
            get { return this.isSingleSetting; }
            set
            {
                this.isSingleSetting = value;
                OnPropertyChanged("IsSingleSetting");
            }
        }

        private MapImageObjectDataInfo dataInfo;

        public MapImageObjectDataInfo DataInfo
        {
            get { return this.dataInfo; }
            set
            {
                if (this.dataInfo == value)
                    return;

                this.dataInfo = value;
            }
        }

        public double ImageOpacity
        {
            get { return this.dataInfo.ImageObjectData.ImageOpacity; }
            set
            {
                this.dataInfo.ImageObjectData.ImageOpacity = value;
                OnPropertyChanged("ImageOpacity");
            }
        }

        #endregion //fields

        #region Command

        private RelayCommand importImagesButtonCommand;
        public ICommand ImportImagesButtonCommand
        {
            get
            {
                return this.importImagesButtonCommand ??
                       (this.importImagesButtonCommand =
                        new RelayCommand(param => this.FileBrowserDialogOpen(), null));
            }
        }

        /// <summary>
        /// File Browser Dialog Open
        /// </summary>
        /// <returns></returns>
        private void FileBrowserDialogOpen()
        {
            var dlg = new OpenFileDialog { FileName = "", Filter = GetImageFilter(), Multiselect = false };

            var result = dlg.ShowDialog();


            if (result.ToString().ToLower() == "ok" || result.ToString().ToLower() == "true")
            {
                var filename = dlg.FileName;
                var replaceFilename = filename.Replace("\\", "/");

                try
                {
                    dataInfo.ImageObjectData.ImageDataStream = ImageStreamContorl.FilePathToString(filename);

                    dataInfo.ImageObjectData.ImageFileName = replaceFilename.Split('/').Last();
                }
                catch (IOException ex)
                {
                    InnowatchDebug.Logger.WriteLine(ex.ToString());
                }


            }
        }

        /// <summary>
        /// Get All Image File Extent Format List
        /// </summary>
        /// <returns></returns>
        private string GetImageFilter()
        {
            var allImageExtensions = new StringBuilder();
            string separator = "";
            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            var images = new Dictionary<string, string>();

            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == "image/jpeg" || codec.MimeType == "image/bmp" || codec.MimeType == "image/gif" || codec.MimeType == "image/png")
                {
                    allImageExtensions.Append(separator);
                    allImageExtensions.Append(codec.FilenameExtension);
                    separator = ";";
                    images.Add(string.Format("{0} Files: ({1})", codec.FormatDescription, codec.FilenameExtension),
                               codec.FilenameExtension);
                }
            }

            var sb = new StringBuilder();

            var isFirst = true;

            foreach (KeyValuePair<string, string> image in images)
            {
                if (!isFirst) sb.AppendFormat("|");
                sb.AppendFormat("{0}|{1}", image.Key, image.Value);
                if (isFirst) isFirst = false;
            }

            return sb.ToString();
        }

        #endregion //Command

    }
}
