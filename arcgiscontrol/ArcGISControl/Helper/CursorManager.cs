using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Win32.SafeHandles;

namespace ArcGISControl.Helper
{
    public enum CursorType
    {
        Move,
        Move2,
        LeftRight,
        UpDown,
        LeftUpRightDown,
        LeftDownRightUp,
        PTZEnable,
        PTZDisable,
        MoveOver,
        BookMark,
        LinkZone,
        HandHold,
        HandOpen,
        Text,
        Line,
        ImageLinkZone,
        WorkStation,
        Image,
        Universal,
        UniversalUrlMapLink,
        UniversalMapLink,
        UniversalUrlLink,
        UniversalHealth,
    };

    public class CursorManager
    {
        private Cursor cursorMove;
        private Cursor cursorMove2;
        private Cursor cursorLeftRight;
        private Cursor cursorUpDown;
        private Cursor cursorLeftUpRightDown;
        private Cursor cursorLeftDownRightUp;
        private Cursor cursorMoveOver;

        private Cursor cursorPTZEnable;
        private Cursor cursorPTZDisable;

        private Cursor cursorBookMark;
        private Cursor cursorLinkZone;
        private Cursor cursorUniversal;

        private Cursor cursorHandHold;
        private Cursor cursorHandOpen;

        private Cursor cursorText;

        private Cursor cursorLine;
        private Cursor cursorImage;
        private Cursor cursorImageLinkZone;
        private Cursor cursorWorkStation;

        private Cursor cursorUniversalUrlMapLink;
        private Cursor cursorUniversalMapLink;
        private Cursor cursorUniversalUrlLink;
        private Cursor cursorUniversalHealth;

        public CursorManager()
        {
            this.CreateCursor();
        }

        private void CreateCursor()
        {
            this.cursorMove = Cursors.Arrow;
            this.cursorMove2 = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorMove, 5, 5);
            this.cursorMoveOver = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorMoveOver, 5, 5);
            this.cursorLeftRight = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorLeftRight, 5, 5);
            this.cursorUpDown = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorUpDown, 5, 5);
            this.cursorLeftUpRightDown = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorLeftUpRightDown, 5, 5);
            this.cursorLeftDownRightUp = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorLeftDownRightUp, 5, 5);

            this.cursorPTZEnable = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorPTZEnable, 5, 5);
            this.cursorPTZDisable = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorPTZDisable, 5, 5);

            this.cursorBookMark = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorBookMark, 13, 40);
            this.cursorLinkZone = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorLinkZone, 1, 1);
            this.cursorUniversal = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorUniversal, 1, 1);

            this.cursorHandHold = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CusorHandHold, 5, 5);
            this.cursorHandOpen = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CusorHandOpen, 5, 5);
            this.cursorText = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorText, 5, 5);
            this.cursorLine = this.CreateCursorWithBitmap((Bitmap) Properties.Resources.CursorLine, 5, 5);
            this.cursorImage = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorImage, 5, 5);
            this.cursorImageLinkZone = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorImageLinkZone, 5, 5);
            this.cursorWorkStation = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorWorkstation, 5, 5);

            this.cursorUniversalUrlMapLink = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorUrlMapLink, 5, 5);
            this.cursorUniversalMapLink = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorMapLink, 5, 5);
            this.cursorUniversalUrlLink = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorUrlLink, 5, 5);
            this.cursorUniversalHealth = this.CreateCursorWithBitmap((Bitmap)Properties.Resources.CursorHealth, 5, 5);
        }

        public Cursor CreateCursorWithBitmap(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            var iconhandle = bmp.GetHicon();
            var tmp = new IconInfo();
            GetIconInfo(iconhandle, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;

            var ptr = CreateIconIndirect(ref tmp);
            var handle = new SafeFileHandle(ptr, false);
            DestroyIcon(iconhandle);

            return CursorInteropHelper.Create(handle);
        }

        public Cursor GetCursor(CursorType cursorType)
        {
            if (cursorType == CursorType.Move)
                return this.cursorMove;
            else if (cursorType == CursorType.LeftRight)
                return this.cursorLeftRight;
            else if (cursorType == CursorType.UpDown)
                return this.cursorUpDown;
            else if (cursorType == CursorType.LeftUpRightDown)
                return this.cursorLeftUpRightDown;
            else if (cursorType == CursorType.LeftDownRightUp)
                return this.cursorLeftDownRightUp;
            else if (cursorType == CursorType.PTZEnable)
                return this.cursorPTZEnable;
            else if (cursorType == CursorType.PTZDisable)
                return this.cursorPTZDisable;
            else if (cursorType == CursorType.MoveOver)
                return this.cursorMoveOver;
            else if (cursorType == CursorType.Move2)
                return this.cursorMove2;
            else if (cursorType == CursorType.BookMark)
                return this.cursorBookMark;
            else if (cursorType == CursorType.LinkZone)
                return this.cursorLinkZone;
            else if (cursorType == CursorType.Universal)
                return this.cursorUniversal;
            else if (cursorType == CursorType.HandOpen)
                return this.cursorHandOpen;
            else if (cursorType == CursorType.HandHold)
                return this.cursorHandHold;
            else if (cursorType == CursorType.Text)
                return this.cursorText;
            else if (cursorType == CursorType.Line)
                return this.cursorLine;
            else if (cursorType == CursorType.Image)
                return this.cursorImage;
            else if (cursorType == CursorType.ImageLinkZone)
                return this.cursorImageLinkZone;
            else if (cursorType == CursorType.WorkStation)
                return this.cursorWorkStation;
            else if (cursorType == CursorType.UniversalUrlMapLink)
                return this.cursorUniversalUrlMapLink;
            else if (cursorType == CursorType.UniversalMapLink)
                return this.cursorUniversalMapLink;
            else if (cursorType == CursorType.UniversalUrlLink)
                return this.cursorUniversalUrlLink;
            else if (cursorType == CursorType.UniversalHealth)
                return this.cursorUniversalHealth;

            return Cursors.Arrow;
        }

        // Singleton
        public static CursorManager Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        // Singleton
        private class Nested
        {
            internal static readonly CursorManager instance = new CursorManager();
        }

        #region Methods

        [DllImport("user32.dll")]
        private static extern IntPtr CreateIconIndirect(ref IconInfo iconInfo);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr iconHandle, ref IconInfo iconInfo);

        #endregion

        /// <summary>
        /// The icon info.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            #region Constants and Fields

            public bool fIcon;

            public int xHotspot;

            public int yHotspot;

            public IntPtr hbmMask;

            public IntPtr hbmColor;

            #endregion
        }
    }
}
