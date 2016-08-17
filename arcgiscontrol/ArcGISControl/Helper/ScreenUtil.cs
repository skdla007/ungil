
namespace ArcGISControl.Helper
{
    using System;
    using System.Collections.Generic;
    using Drawing = System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Media;

    public static class ScreenUtil
    {
        private static Point dpiPoint;

        static ScreenUtil()
        {
            // 현재 모니터의 Dpi 값을 찾아 보관한다.
            // Dpi 값은 Windows 로그온 시 한 번만 적용되므로 미리 저장한다. (DPI 변경 시 Windows 로그 오프해야함.)
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            var dpiX = ((int)dpiXProperty.GetValue(null, null)) / 96.0;
            var dpiY = ((int)dpiYProperty.GetValue(null, null)) / 96.0;

            dpiPoint = new Point(dpiX, dpiY);
        }

        public static Rect GetPrimaryScreenRegion()
        {
            return new Rect(Screen.PrimaryScreen.Bounds.Left,
                Screen.PrimaryScreen.Bounds.Top,
                Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height);
        }

        /// <summary>
        /// 전체 화면 영역을 구하는 함수
        /// </summary>
        /// <returns>전체 화면 영역을 나타내는 값이 반환된다.</returns>
        public static Rect GetTotalScreenBound()
        {
            var resultRect = new Rect();
            var dpiX = dpiPoint.X;
            var dpiY = dpiPoint.Y;
            
            foreach (var screen in Screen.AllScreens)
            {
                resultRect.Union(
                    new Rect(
                        screen.Bounds.X / dpiX, 
                        screen.Bounds.Y / dpiY, 
                        screen.Bounds.Width / dpiX, 
                        screen.Bounds.Height / dpiY));
            }

            return resultRect;
        }

        public static Rect GetEnclosingRect(List<Rect> playerList)
        {
            var x = playerList.Min(scr => scr.Left);
            var y = playerList.Min(scr => scr.Top);
            var width = playerList.Max(scr => scr.Right) - x;
            var height = playerList.Max(scr => scr.Bottom) - y;

            return new Rect(x, y, width, height);
        }

        /// <summary>
        ///   화면에 나타날 Rect가 스크린에 어떻게 잘려서 나타날지 구한다.
        ///   입력과 출력 모두 Pixel 단위를 사용한다.
        /// </summary>
        /// <param name="originRect"> 스크린 좌표계로 표현한 그리고 싶은 Rect </param>
        /// <returns>
        /// 사각형 두 개의 순서쌍의 목록을 리턴한다.
        /// 첫번째 사각형은 잘린 사각형을 스크린 좌표계로 표현한 것이고,
        /// 두번째 사각형은 잘린 사각형을 해당 스크린의 스크린 기준 상대좌표로 나타낸 것이다.
        /// </returns>
        public static List<Tuple<Rect, Rect>> GetTotalRectanglesByMonitor(Rect originRect)
        {
            var returnRectList = new List<Tuple<Rect, Rect>>();

            foreach (var scr in Screen.AllScreens)
            {
                var bound = new Rect(scr.Bounds.X, scr.Bounds.Y, scr.Bounds.Width, scr.Bounds.Height);
                if (!bound.IntersectsWith(originRect)) continue;
                bound.Intersect(originRect);
                var relativeLocation = new Point(bound.X, bound.Y);
                relativeLocation.Offset(-scr.Bounds.X, -scr.Bounds.Y);
                returnRectList.Add(new Tuple<Rect, Rect>(bound, new Rect(relativeLocation, bound.Size)));
            }

            return returnRectList;
        }

        /// <summary>
        ///   화면에 나타날 Rect가 스크린에 어떻게 잘려서 나타날지 구한다.
        ///   입력과 출력 모두 Pixel 단위를 사용한다.
        /// </summary>
        /// <param name="originRect"> 스크린 좌표계로 표현한 그리고 싶은 Rect </param>
        /// <returns>
        /// 사각형 두 개의 순서쌍의 목록을 리턴한다.
        /// 첫번째 사각형은 잘린 사각형을 스크린 좌표계로 표현한 것이고,
        /// 두번째 사각형은 잘린 사각형을 해당 스크린의 스크린 기준 상대좌표로 나타낸 것이다.
        /// </returns>
        public static List<Tuple<Int32Rect, Int32Rect>> GetTotalRectanglesByMonitor(Int32Rect originRect)
        {
            var originRectangle = new Drawing.Rectangle(originRect.X, originRect.Y, originRect.Width, originRect.Height);
            var returnRectList = new List<Tuple<Int32Rect, Int32Rect>>();

            foreach (var scr in Screen.AllScreens)
            {
                var bound = scr.Bounds;
                if (!bound.IntersectsWith(originRectangle)) continue;
                bound.Intersect(originRectangle);
                var relativeLocation = bound.Location;
                relativeLocation.Offset(-scr.Bounds.X, -scr.Bounds.Y);

                var absoluteRect = new Int32Rect(bound.X, bound.Y, bound.Width, bound.Height);
                var relativeRect = new Int32Rect(relativeLocation.X, relativeLocation.Y, bound.Width, bound.Height);
                returnRectList.Add(new Tuple<Int32Rect, Int32Rect>(absoluteRect, relativeRect));
            }

            return returnRectList;
        }

        /// <summary>
        /// Grid의 중심점을 포함하고 있는 모니터의 시작점을 반환하는 함수
        /// </summary>
        /// <returns>Grid의 중심점을 포함하고 있는 모니터의 시작점을 반환한다.</returns>
        public static Point? GetScreenStartPosition(int centerX, int centerY)
        {
            foreach (var screen in Screen.AllScreens)
            {
                if (centerX >= screen.Bounds.Left &&
                    centerX < screen.Bounds.Right &&
                    centerY >= screen.Bounds.Top &&
                    centerY < screen.Bounds.Bottom)
                {
                    return new Point(screen.Bounds.Location.X, screen.Bounds.Location.Y);
                }
            }

            return null;
        }

        public static Screen GetLocatedScreen(FrameworkElement element)
        {
            // Popup 될 Monitor 결정.
            // 전체 모니터 영역에 MainWindow의 중심을 intersect하여 가장 많이 포함되어 있는 모니터 결정.
            var dpi = GetDpiRatioForDiu();
            var monitorDictionary = new Dictionary<Screen, Rect>();
            var mainWindowLeftTop = element.PointToScreenDIU(new Point(0, 0));
            var mainWindowRect = new Rect(mainWindowLeftTop.X, mainWindowLeftTop.Y, element.ActualWidth, element.ActualHeight);
            foreach (var monitor in Screen.AllScreens)
            {
                var monitorRect =
                    new Rect(
                        monitor.Bounds.X / dpi.X,
                        monitor.Bounds.Y / dpi.Y,
                        monitor.Bounds.Width / dpi.X,
                        monitor.Bounds.Height / dpi.Y); // dpi 비율이 적용된 모니터 영역을 생성한다.
                var intersected = Rect.Intersect(monitorRect, mainWindowRect);
                monitorDictionary.Add(monitor, (intersected.Width > 0 && intersected.Height > 0) ? intersected : new Rect());
            }

            var selectedMonitors = monitorDictionary.OrderByDescending(dic => dic, new RectComparer()).ToList();
            if (selectedMonitors.Count <= 0)
            {
                return Screen.AllScreens[0];
            }

            return selectedMonitors[0].Key;
        }

        /// <summary>
        /// 프로그램 구동 시 저장했던 DPI 대비 DIU 비율을 반환한다.
        /// </summary>
        /// <returns></returns>
        public static Point GetDpiRatioForDiu()
        {
            return dpiPoint;
        }

        /// <summary>
        /// 현재 지정된 객체에서 측정한 DPI 대비 DIU 비율을 반환한다.
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static Point GetCurrentDpiRatioForDiu(Visual visual)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            if (presentationSource == null)
            {
                return new Point(1, 1);
            }

            if (presentationSource.CompositionTarget == null)
            {
                return new Point(1, 1);
            }

            var matrix = presentationSource.CompositionTarget.TransformToDevice;

            return new Point(matrix.M11, matrix.M22);
        }

        /// <summary>
        /// 현재 모니터의 DPI 값을 반환한다.
        /// </summary>
        /// <param name="visual"></param>
        /// <returns></returns>
        public static Point GetVisualDpi(Visual visual)
        {
            var dpi = GetCurrentDpiRatioForDiu(visual);

            return new Point((int)dpi.X * 96, (int)dpi.Y * 96);
        }




        public static Vector DIUToPixels(Vector v)
        {
            return new Vector(v.X * dpiPoint.X, v.Y * dpiPoint.Y);
        }

        public static Point DIUToPixels(Point p)
        {
            return new Point(p.X * dpiPoint.X, p.Y * dpiPoint.Y);
        }

        public static Size DIUToPixels(Size p)
        {
            var res = DIUToPixels(new Vector(p.Width, p.Height));
            return new Size(res.X, res.Y);
        }

        public static Rect DIUToPixels(Rect r)
        {
            return new Rect(
                r.X * dpiPoint.X,
                r.Y * dpiPoint.Y,
                r.Width * dpiPoint.X,
                r.Height * dpiPoint.Y );
        }

        public static Vector PixelsToDIU(Vector v)
        {
            return new Vector(v.X / dpiPoint.X, v.Y / dpiPoint.Y);
        }

        public static Point PixelsToDIU(Point p)
        {
            return new Point(p.X / dpiPoint.X, p.Y / dpiPoint.Y);
        }

        public static Size PixelsToDIU(Size p)
        {
            var res = PixelsToDIU(new Vector(p.Width, p.Height));
            return new Size(res.X, res.Y);
        }

        public static Rect PixelsToDIU(Rect r)
        {
            return new Rect(
                r.X / dpiPoint.X,
                r.Y / dpiPoint.Y,
                r.Width / dpiPoint.X,
                r.Height / dpiPoint.Y);
        }
    }
}
