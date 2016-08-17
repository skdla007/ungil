using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ArcGISControl.Helper
{
    public class ImageStreamContorl
    {
        private static readonly int defaultImageWidth = 1920, defaultImageHegiht = 1080;

        public static string ResourceUriToStream(Bitmap bitmap)
        {
            MemoryStream ms = null;
            string base64String = null;

            try
            {
                using (ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);

                    // Convert Image to byte[]
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    base64String = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                ms = null;
            }

            return base64String;
        }

        public static string FilePathToString(string filePath)
        {
            MemoryStream ms = null;
            string base64String = null;

            try
            {
                using (ms = new MemoryStream())
                {
                    var image = Image.FromFile(@filePath, true);

                    int sourceWidth = image.Width;
                    int sourceHeight = image.Height;

                    if (defaultImageHegiht < image.Height || defaultImageWidth < image.Width)
                    {
                        if (image.Height > image.Width)
                        {
                            sourceHeight = image.Height;
                            sourceWidth = (int)(((double)sourceWidth / (double)sourceHeight) * (double)sourceWidth);
                        }
                        else
                        {
                            sourceHeight = (int)(((double)sourceHeight / (double)sourceWidth) * (double)sourceHeight);
                            sourceWidth = image.Width;
                        }
                    }

                    var bitmap  = (sourceWidth == image.Width && sourceHeight == image.Height) ?
                                    new Bitmap(image) : new Bitmap(image, sourceWidth, sourceHeight);
                    bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    bitmap.Save(ms, ImageFormat.Png);
                    
                    // Convert Image to byte[]
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    base64String = Convert.ToBase64String(imageBytes);
                }
            }
            catch (Exception ex)
            {
                InnowatchDebug.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                ms = null;
            }

            return base64String;
        }

        public static BitmapImage FileSteamToImage(string fileStream)
        {
            var bitmapImage = new BitmapImage();
            MemoryStream ms = null;
            MemoryStream readMemoryStream = null;

            try
            {
                // Convert Base64 String to byte[]
                var imageBytes = Convert.FromBase64String(fileStream);
                using (ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                {
                    ms.Write(imageBytes, 0, imageBytes.Length);

                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = null;
                    bitmapImage.StreamSource = ms;
                    bitmapImage.EndInit();
                }
            }
            catch (Exception ex)
            {   
                InnowatchDebug.Logger.WriteLine(ex.ToString());
            }
            finally
            {
                if (ms != null)
                    ms.Close();
            }

            return bitmapImage;
        }
    }
}
