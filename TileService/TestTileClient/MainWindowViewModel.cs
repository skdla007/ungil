using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace TestTileClient
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool StopFlag { get; set; }

        private ObservableCollection<Image> tileImageCollection;
        public ObservableCollection<Image> TileImageCollection
        {
            get
            {
                return this.tileImageCollection;
            }
            set
            {
                this.tileImageCollection = value;
                this.OnPropertyChanged("TileImageCollection");
            }
        }

        private RelayCommand startCommand;

        public ICommand StartCommand
        {
            get
            {
                if (this.startCommand == null)
                {
                    this.startCommand = new RelayCommand(
                        param => this.Start());
                }

                return this.startCommand;
            }
        }

        private RelayCommand stopCommand;

        public ICommand StopCommand
        {
            get
            {
                if (this.stopCommand == null)
                {
                    this.stopCommand = new RelayCommand(
                        param => this.Stop());
                }

                return this.stopCommand;
            }
        }

        public void Start()
        {
           TileServiceCall();
        }

        public void TileServiceCall()
        {
            this.StopFlag = false;

            Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    if (this.StopFlag) return;

                    Stream stream = this.HttpWebRequestAsync();

                    //bitmapImage.Freeze();

                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Image image = new Image();

                        image.Source = BitmapFrame.Create(stream,
                                      BitmapCreateOptions.None,
                                      BitmapCacheOption.OnLoad);

                        this.TileImageCollection.Add(image);
                    }));
                }
            });
        }

        public void Stop()
        {
            this.StopFlag = true;
        }


        public Stream HttpWebRequestAsync()
        {
            try
            {
                // The downloaded resource ends up in the variable named content.
                MemoryStream content = new MemoryStream();

                // Initialize an HttpWebRequest for the current URL.
                var webReq = (HttpWebRequest)WebRequest.Create("http://172.16.10.56:30000/rest/tile/");
                webReq.Method = "GET";
                webReq.ContentType = "image/jpg";

                using (WebResponse response = webReq.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        //await responseStream.CopyToAsync(content);
                        return responseStream;
                    }
                }

                // Return the result as a image.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;

            if (handler == null) return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // PropertyChanged
    }
}
