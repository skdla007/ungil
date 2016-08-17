
namespace ArcGISControls.CommonData.Windows
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// 윈도의 Chrome(타이틀바, Resize Border 등)을 제거하는 기능을 제공해주는 클래스
    /// </summary>
    /// <remarks> 
    ///  Callers must have UIPermission(UIPermissionWindow.AllWindows) to call this API.
    /// </remarks> 
    /// <SecurityNote>
    ///  Critical - uses a critical field.
    ///  PublicOK - as there's a demand.
    /// </SecurityNote> 
    public class ChromeRemover : IDisposable
    {
        private HwndSource hwndSource;

        private readonly Window window;

        private bool haveSetRegion;

        public CornerRadius CornerRadius { get; set; }

        public Thickness ResizeBorderWidth { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="window">Chrome을 제거할 Window</param>
        public ChromeRemover(Window window)
        {
            this.window = window;
        }

        /// <summary>
        /// 지정된 윈도의 Chrome을 제거한다.
        /// 호출 시점은 OnSourceInitialized에서 해주는 것이 좋다.
        /// 그 이전에는 Handle을 못 얻어올 수도 있고, 그 이후에는 너무 늦다.
        /// 한번만 작업해야한다.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">window가 null일 때</exception>
        /// <exception cref="System.InvalidOperationException">window에서 Handle을 얻어올 수 없을 때, 이미 Apply()가 불린 경우</exception>
        public void Apply()
        {
            if (this.hwndSource != null)
                throw new InvalidOperationException("already applied");

            this.hwndSource = CheckWindowParamAndGetHwndSource(window);
            this.hwndSource.AddHook(_WndProc);
        }

        /// <summary>
        /// Apply() 메서드를 통해 한 작업을 되돌린다.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">window가 null일 때</exception>
        /// <exception cref="System.InvalidOperationException">window에서 Handle을 얻어올 수 없을 때</exception>
        public void Restore()
        {
            if (hwndSource == null)
                return;

            this.hwndSource.RemoveHook(_WndProc);
            this.hwndSource = null;
        }

        protected static HwndSource CheckWindowParamAndGetHwndSource(Window window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("unable to get the handle of the window");
            var hwnd = HwndSource.FromHwnd(handle);
            if (hwnd == null)
                throw new InvalidOperationException("unable to get the hwnd from the window");

            return hwnd;
        }

        protected IntPtr _WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.NCACTIVATE:
                    {
                        // 기본적으로 non client area에 강제로 Deactivated 됐거나
                        // Activated 됐을 때 상태를 표시하려고 덧그리기 때문에
                        // 아래와 같이 lParam에 -1을 넣어서 처리를 막는다.
                        var ret = NativeMethods.DefWindowProc(hwnd, msg, wParam, new IntPtr(-1));
                        handled = true;
                        return ret;
                    }
                case WM.NCCALCSIZE:
                    {
                        // 아래와 같이 받은 걸 그대로 넘겨버리면 non client area 크기가 없게 된다.
                        handled = true;
                        return new IntPtr((int)WVR.REDRAW);
                    }
                case WM.GETMINMAXINFO:
                    {
                        // Maximize를 하게 되면 모니터 작업 영역으로 정확히 채워주지 않는다.
                        // 처리를 하지 않으면 최대화 했을 때 
                        // 따라서 Maximize 하는 위치를 모니터 작업 영역으로 정확히 수정한다.
                        var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
                        const int MONITOR_DEFAULTTONEAREST = 0x00000002;
                        var monitor = NativeMethods.MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
                        if (monitor != IntPtr.Zero)
                        {
                            var monitorInfo = new MONITORINFO();
                            NativeMethods.GetMonitorInfo(monitor, monitorInfo);
                            var rcWorkArea = monitorInfo.rcWork;
                            var rcMonitorArea = monitorInfo.rcMonitor;
                            mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                            mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                            mmi.ptMaxSize.x = Math.Abs(rcWorkArea.Width);
                            mmi.ptMaxSize.y = Math.Abs(rcWorkArea.Height);
                        }

                        Marshal.StructureToPtr(mmi, lParam, false);
                        handled = false;
                        return IntPtr.Zero;
                    }
                case WM.WINDOWPOSCHANGED:
                    {
                        var diamtr1 = (int)Math.Ceiling(CornerRadius.TopLeft * 2);
                        var diamtr2 = (int)Math.Ceiling(CornerRadius.TopRight * 2);
                        var diamtr3 = (int)Math.Ceiling(CornerRadius.BottomLeft * 2);
                        var diamtr4 = (int)Math.Ceiling(CornerRadius.BottomRight * 2);

                        // no clipping
                        if (diamtr1 <= 0 && diamtr2 <= 0 && diamtr3 <= 0 && diamtr4 <= 0)
                        {
                            this.ClearWindowRegion(hwnd);
                            break;
                        }

                        var wndpl = new WINDOWPLACEMENT();
                        if (!NativeMethods.GetWindowPlacement(hwnd, wndpl))
                        {
                            this.ClearWindowRegion(hwnd);
                            break;
                        }

                        if (wndpl.showCmd == SW.SHOWMAXIMIZED)
                        {
                            // maximize 상태에서 뚫린 곳이 있으면 이상하다.
                            this.ClearWindowRegion(hwnd);
                            break;
                        }

                        IntPtr hrgn;

                        var wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        // uniform radius
                        if (diamtr1 == diamtr2 &&
                            diamtr2 == diamtr3 &&
                            diamtr3 == diamtr4)
                        {
                            hrgn = NativeMethods.CreateRoundRectRgn(0, 0, wp.cx, wp.cy, 20, 20);
                        }
                        else
                        {
                            var maxDiamtr = Math.Max(Math.Max(Math.Max(diamtr1, diamtr2), diamtr3), diamtr4);
                            hrgn = NativeMethods.CreateRoundRectRgn(0, 0, wp.cx, wp.cy, maxDiamtr, maxDiamtr);
                            if (maxDiamtr != diamtr1)
                            {
                                var hrgn2 = NativeMethods.CreateRoundRectRgn(0, 0, wp.cx / 2, wp.cy / 2, diamtr1, diamtr1);
                                if (hrgn2 != IntPtr.Zero)
                                {
                                    NativeMethods.CombineRgn(hrgn, hrgn, hrgn2, RGN.OR);
                                    NativeMethods.DeleteObject(hrgn2);
                                }
                            }
                            if (maxDiamtr != diamtr2)
                            {
                                var hrgn2 = NativeMethods.CreateRoundRectRgn(wp.cx / 2, 0, wp.cx, wp.cy / 2, diamtr2, diamtr2);
                                if (hrgn2 != IntPtr.Zero)
                                {
                                    NativeMethods.CombineRgn(hrgn, hrgn, hrgn2, RGN.OR);
                                    NativeMethods.DeleteObject(hrgn2);
                                }
                            }
                            if (maxDiamtr != diamtr3)
                            {
                                var hrgn2 = NativeMethods.CreateRoundRectRgn(0, wp.cy / 2, wp.cx / 2, wp.cy, diamtr3, diamtr3);
                                if (hrgn2 != IntPtr.Zero)
                                {
                                    NativeMethods.CombineRgn(hrgn, hrgn, hrgn2, RGN.OR);
                                    NativeMethods.DeleteObject(hrgn2);
                                }
                            }
                            if (maxDiamtr != diamtr4)
                            {
                                var hrgn2 = NativeMethods.CreateRoundRectRgn(wp.cx / 2, wp.cy / 2, wp.cx, wp.cy, diamtr4, diamtr4);
                                if (hrgn2 != IntPtr.Zero)
                                {
                                    NativeMethods.CombineRgn(hrgn, hrgn, hrgn2, RGN.OR);
                                    NativeMethods.DeleteObject(hrgn2);
                                }
                            }
                        }

                        this.SetWindowRegion(hwnd, hrgn);

                        handled = false;
                        return IntPtr.Zero;
                    }
                case WM.NCHITTEST:
                    {
                        var wndpl = new WINDOWPLACEMENT();
                        if (NativeMethods.GetWindowPlacement(hwnd, wndpl))
                        {
                            if (wndpl.showCmd == SW.SHOWMAXIMIZED)
                            {
                                return IntPtr.Zero;
                            }
                        }

                        var x = (short)lParam.ToInt64();
                        var y = (short)(lParam.ToInt64() >> 16);
                        RECT windowRect;
                        if (!NativeMethods.GetWindowRect(hwnd, out windowRect))
                        {
                            return IntPtr.Zero;
                        }
                        if (windowRect.Left <= x && x < windowRect.Right &&
                            windowRect.Top <= y && y < windowRect.Bottom)
                        {
                            var left = x - windowRect.Left + 1 <= this.ResizeBorderWidth.Left;
                            var top = y - windowRect.Top + 1 <= this.ResizeBorderWidth.Top;
                            var right = windowRect.Right - x <= this.ResizeBorderWidth.Right;
                            var bottom = windowRect.Bottom - y <= this.ResizeBorderWidth.Bottom;

                            if (top && left)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTTOPLEFT;
                            }
                            if (top && right)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTTOPRIGHT;
                            }
                            if (bottom && left)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTBOTTOMLEFT;
                            }
                            if (bottom && right)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTBOTTOMRIGHT;
                            }
                            if (top)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTTOP;
                            }
                            if (bottom)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTBOTTOM;
                            }
                            if (left)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTLEFT;
                            }
                            if (right)
                            {
                                handled = true;
                                return (IntPtr)HitTest.HTRIGHT;
                            }
                        }
                        return IntPtr.Zero;
                    }
            }
            return IntPtr.Zero;
        }

        private void ClearWindowRegion(IntPtr hwnd)
        {
            if (this.haveSetRegion)
            {
                NativeMethods.SetWindowRgn(hwnd, IntPtr.Zero, true);
            }

            this.haveSetRegion = false;
        }

        private bool SetWindowRegion(IntPtr hwnd, IntPtr hrgn)
        {
            if (hrgn != IntPtr.Zero &&
                0 == NativeMethods.SetWindowRgn(hwnd, hrgn, true))
            {
                NativeMethods.DeleteObject(hrgn);
                return false;
            }

            this.haveSetRegion = true;
            return true;
        }

        #region IDisposable 구현
        private bool disposed;

        ~ChromeRemover()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;

            this.disposed = true;
            if (disposing)
            {
                this.Restore();
            }
        }
        #endregion IDisposable 구현
    }
}
