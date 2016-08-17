using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGISControl.TiledMapLayer
{
    internal class CoordinateConverter
    {
        #region Naver

        static private double D2R = 0.0174532925199;
        static private double R2D = 57.2957795131;
        //static private double PI = Math.PI;
        static private double TWO_PI = Math.PI * 2;
        static private double EPSLN = 1.0e-10;

        public CoordinateConverter()
        {
        }

        public Point GetWGS84FromUTMK(Point pUtmK)
        {
            var pWgs84 = new Point();
            SetUTMK();

            InitTMerc();
            pWgs84 = InverseTMerc(pUtmK);
            pWgs84.X *= R2D;
            pWgs84.Y *= R2D;
            return pWgs84;
        }

        public Point GetUTMKFromWGS84(Point pWgs84)
        {
            Point pUtmK = new Point();
            SetUTMK();

            pWgs84.X *= D2R;
            pWgs84.Y *= D2R;

            /**
                Transverse Mercator Forward  - long/lat to x/y
                long/lat in radians
            */
            InitTMerc();
            pUtmK = ForwardTMerc(pWgs84);
            return pUtmK;
        }

        private Point ForwardTMerc(Point p)
        {
            double delta_lon = AdjustLon(p.X - long0); // Delta longitude
            double con;    // cone constant
            double x, y;
            double sin_phi = Math.Sin(p.Y);
            double cos_phi = Math.Cos(p.Y);
            double al = cos_phi * delta_lon;
            double als = Math.Pow(al, 2);
            double c = ep2 * Math.Pow(cos_phi, 2);
            double tq = Math.Tan(p.Y);
            double t = Math.Pow(tq, 2);
            con = 1.0 - es * Math.Pow(sin_phi, 2);
            double n = a / Math.Sqrt(con);
            double ml = a * mlfn(e0, e1, e2, e3, p.Y);
            x = k0 * n * al * (1.0 + als / 6.0 * (1.0 - t + c + als / 20.0 * (5.0 - 18.0 * t + Math.Pow(t, 2) + 72.0 * c - 58.0 * ep2))) + x0;
            y = k0 * (ml - ml0 + n * tq * (als * (0.5 + als / 24.0 * (5.0 - t + 9.0 * c + 4.0 * Math.Pow(c, 2) + als / 30.0 * (61.0 - 58.0 * t + Math.Pow(t, 2) + 600.0 * c - 330.0 * ep2))))) + y0;
            //Debug.WriteLine("ForwardTMerc - p : " + p + ",ml : "  + ml);
            //Debug.WriteLine("ForwardTMerc - x : " + x + ",y : "  + y);
            p.X = x;
            p.Y = y;
            return p;
        }

        private Point InverseTMerc(Point p)
        {
            double con, phi;  /* temporary angles       */
            double delta_phi; /* difference between longitudes    */
            double i;
            double max_iter = 6;      /* maximun number of iterations */
            double lat, lon;

            p.X -= x0;
            p.Y -= y0;
            con = (ml0 + p.Y / k0) / a;
            phi = con;
            for (i = 0; ; i++)
            {
                delta_phi = ((con + e1 * Math.Sin(2.0 * phi) - e2 * Math.Sin(4.0 * phi) + e3 * Math.Sin(6.0 * phi)) / e0) - phi;
                phi += delta_phi;
                if (Math.Abs(delta_phi) <= EPSLN) break;
                if (i >= max_iter)
                {
                    Debug.WriteLine("Error in tm2ll(): Latitude failed to converge");
                    return new Point(0, 0);
                }

            }
            double sin_phi = Math.Sin(phi);
            double cos_phi = Math.Cos(phi);
            double tan_phi = Math.Tan(phi);
            double c = ep2 * Math.Pow(cos_phi, 2);
            double cs = Math.Pow(c, 2);
            double t = Math.Pow(tan_phi, 2);
            double ts = Math.Pow(t, 2);
            con = 1.0 - es * Math.Pow(sin_phi, 2);
            double n = a / Math.Sqrt(con);
            double r = n * (1.0 - es) / con;
            double d = p.X / (n * k0);
            double ds = Math.Pow(d, 2);

            lat = phi - (n * tan_phi * ds / r) * (0.5 - ds / 24.0 * (5.0 + 3.0 * t + 10.0 * c - 4.0 * cs - 9.0 * ep2 - ds / 30.0 * (61.0 + 90.0 * t + 298.0 * c + 45.0 * ts - 252.0 * ep2 - 3.0 * cs)));
            lon = AdjustLon(long0 + (d * (1.0 - ds / 6.0 * (1.0 + 2.0 * t + c - ds / 20.0 * (5.0 - 2.0 * c + 28.0 * t - 3.0 * cs + 8.0 * ep2 + 24.0 * ts))) / cos_phi));
            p.X = lon;
            p.Y = lat;
            return p;
        }

        private double long0;
        private double lat0;
        private double lat_0;
        private double lon_0;
        private double ep2;
        private double a;
        private double b;
        private double a2;
        private double b2;
        private double es;
        private double k0;
        private double x0;
        private double y0;

        private void SetUTMK()
        {
            lat_0 = 38.0;
            lon_0 = 127.5;
            long0 = lon_0 * D2R;
            lat0 = lat_0 * D2R;
            a = 6378137.0;
            b = 6356752.3141403;
            k0 = 0.9996;
            x0 = 1000000.0;
            y0 = 2000000.0;
            a2 = a * a;          // used in geocentric
            b2 = b * b;          // used in geocentric
            ep2 = (a2 - b2) / b2; // used in geocentric
            es = (a2 - b2) / a2;  // e ^ 2 //this.es=1-(Math.pow(this.b,2)/Math.pow(this.a,2));
        }

        private double e0;
        private double e1;
        private double e2;
        private double e3;
        private double ml0;
        private double ind;

        private void InitTMerc()
        {
            e0 = e0fn(es);
            e1 = e1fn(es);
            e2 = e2fn(es);
            e3 = e3fn(es);
            ml0 = a * mlfn(e0, e1, e2, e3, lat0);
            ind = (es < 0.00001) ? 1 : 0; // spherical?
        }

        private double AdjustLon(double lng)
        {
            lng = (Math.Abs(lng) < Math.PI) ? lng : (lng - (GetSign(lng) * TWO_PI));
            return (lng);
        }
        private double GetSign(double x)
        {
            if (x < 0.0) return (-1); else return (1);
        }
        private double e0fn(double x) { return (1.0 - 0.25 * x * (1.0 + x / 16.0 * (3.0 + 1.25 * x))); }
        private double e1fn(double x) { return (0.375 * x * (1.0 + 0.25 * x * (1.0 + 0.46875 * x))); }
        private double e2fn(double x) { return (0.05859375 * x * x * (1.0 + 0.75 * x)); }
        private double e3fn(double x) { return (x * x * x * (35.0 / 3072.0)); }
        private double mlfn(double e0, double e1, double e2, double e3, double phi) { return (e0 * phi - e1 * Math.Sin(2.0 * phi) + e2 * Math.Sin(4.0 * phi) - e3 * Math.Sin(6.0 * phi)); }

        #endregion //Naver

        #region Others

        #region Map Type

        //경위도 좌표계 (타원체) , 대부분 통용되는 기준 타원체
        //Google Earth
        public static int COORD_TYPE_WGS84 = 5;
        public static int COORD_TYPE_BESSEL = 6;

        public static int COORD_TYPE_TM = 1;
        public static int COORD_TYPE_KTM = 2;
        
        public static int COORD_TYPE_UTM = 3;
        
        //Daum Map - Bessel 기반
        public static int COORD_TYPE_CONGNAMUL = 4;
       
        public static int COORD_TYPE_WTM = 7;
        public static int COORD_TYPE_WKTM = 8;
        
        //Daum Map - Wgs84 rlqks
        public static int COORD_TYPE_WCONGNAMUL = 10;

        //Google Mercator 제공 해야함
        //Meractor는 제공 ArcGIS에서 처리 하고 있음(Bing Map)

        #endregion //Map Type

        #region Map Base Point

        public static double BASE_TM_LON = 127.0D;
        public static double BASE_TM_LAT = 38.0D;
        public static double BASE_KTM_LON = 128.0D;
        public static double BASE_KTM_LAT = 38.0D;
        public static double BASE_UTM_LON = 129.0D;
        public static double BASE_UTM_LAT = 0.0D;

        #endregion //Map Base Point

        private static int[,] COORD_BASE = new int[,]{ 
                                            {new int(),new int()}, 
                                            { 127, 38 }, 
                                            { -1, -1 }, 
                                            { 129, 38 }, 
                                            { -1, -1 }, 
                                            { -1, -1 }, 
                                            { -1, -1 }, 
                                            { 127, 38 }, 
                                            { -1, -1 }, 
                                            {new int(),new int()}, 
                                            { -1, -1 } };

        public static CoordPoint getTransCoord(CoordPoint inPoint, int fromType, int toType)
        {
            return convertCoord(inPoint, fromType, toType, COORD_BASE[fromType, 0],
              COORD_BASE[fromType, 1], COORD_BASE[toType, 0],
              COORD_BASE[toType, 1]);
        }

        private static CoordPoint convertCoord(CoordPoint point, int fromType, int toType, double frombx, double fromby, double tobx, double toby)
        {
            CoordPoint transPt = null;
            double bx = frombx;

            switch (fromType)
            {
                case 1:
                    if (frombx <= 0.0D)
                    {
                        bx = 127.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertTM2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 2:
                    if (frombx <= 0.0D)
                    {
                        bx = 128.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertKTM2(point, toType, tobx, toby);
                    break;
                case 3:
                    if (frombx <= 0.0D)
                    {
                        bx = 129.0D;
                        fromby = 0.0D;
                    }
                    transPt = convertUTM2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 4:
                    if (frombx <= 0.0D)
                    {
                        bx = 127.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertCONGNAMUL2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 5:
                    transPt = convertWGS2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 6:
                    transPt = convertBESSEL2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 7:
                    if (frombx <= 0.0D)
                    {
                        bx = 127.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertWTM2(point, toType, bx, fromby, tobx, toby);
                    break;
                case 8:
                    if (frombx <= 0.0D)
                    {
                        bx = 128.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertWKTM2(point, toType, bx, frombx, tobx, toby);
                    break;
                case 9:
                case 10:
                    if (frombx <= 0.0D)
                    {
                        bx = 127.0D;
                        fromby = 38.0D;
                    }
                    transPt = convertWCONGNAMUL2(point, toType, bx, fromby, tobx, toby);
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertTM2(CoordPoint point, int toType, double frombx, double fromby, double tobx, double toby)
        {
            CoordPoint transPt = point.clone();
            switch (toType)
            {
                case 1:
                    if (tobx <= 0.0D)
                    {
                        tobx = 127.0D;
                        toby = 38.0D;
                    }
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2TM(tobx, toby);
                    break;
                case 2:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (tobx <= 0.0D)
                    {
                        tobx = 129.0D;
                        toby = 0.0D;
                    }
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2UTM(tobx, toby);
                    break;
                case 4:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    break;
                case 7:
                    if (tobx <= 0.0D)
                    {
                        tobx = 127.0D;
                        toby = 38.0D;
                    }
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WTM(tobx, toby);
                    break;
                case 8:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertTM2BESSEL(frombx, fromby);
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WCONG();
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertKTM2(CoordPoint point, int toType, double tobx, double toby)
        {
            CoordPoint transPt = point.clone();

            switch (toType)
            {
                case 1:
                    if (tobx <= 0.0D)
                    {
                        tobx = 127.0D;
                        toby = 38.0D;
                    }
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2TM(tobx, toby);
                    break;
                case 2:
                    break;
                case 3:
                    if (tobx <= 0.0D)
                    {
                        tobx = 129.0D;
                        toby = 0.0D;
                    }
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2UTM(tobx, toby);
                    break;
                case 4:
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertKTM2BESSEL();
                    break;
                case 7:
                    if (tobx <= 0.0D)
                    {
                        tobx = 127.0D;
                        toby = 38.0D;
                    }
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WTM(tobx, toby);
                    break;
                case 8:
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertKTM2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WCONG();
                    break;
            }
            return transPt;
        }

        private static CoordPoint convertUTM2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();

            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertUTM2WGS(e, h);
                    break;
                case 6:
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertUTM2WGS(e, h);
                    transPt.convertWGS2WCONG();
                    break;
            }
            return transPt;
        }

        private static CoordPoint convertCONGNAMUL2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();

            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    break;
                case 5:
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertCONG2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertCONG2BESSEL();
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WCONG();
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertWGS2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();
            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    break;
                case 6:
                    transPt.convertWGS2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertWGS2WCONG();
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertBESSEL2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();
            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertBESSEL2WGS();
                    transPt.convertWGS2WCONG();
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertWTM2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();
            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertWTM2WGS(e, h);
                    transPt.convertWGS2WCONG();
                    break;
            }

            return transPt;
        }

        private static CoordPoint convertWKTM2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();
            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    transPt.convertWKTM2WGS();
                    transPt.convertWGS2WCONG();

                    break;
            }

            return transPt;
        }

        private static CoordPoint convertWCONGNAMUL2(CoordPoint point, int d, double e, double h, double g, double j)
        {
            CoordPoint transPt = point.clone();
            switch (d)
            {
                case 1:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2TM(g, j);
                    break;
                case 2:
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2KTM();
                    break;
                case 3:
                    if (g <= 0.0D)
                    {
                        g = 129.0D;
                        j = 0.0D;
                    }
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2UTM(g, j);
                    break;
                case 4:
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2CONG();
                    break;
                case 5:
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2BESSEL();
                    transPt.convertBESSEL2WGS();
                    break;
                case 6:
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2BESSEL();
                    break;
                case 7:
                    if (g <= 0.0D)
                    {
                        g = 127.0D;
                        j = 38.0D;
                    }
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2WTM(g, j);
                    break;
                case 8:
                    transPt.convertWCONG2WGS();
                    transPt.convertWGS2WKTM();
                    break;
                case 9:
                case 10:
                    break;
            }
            return transPt;
        }

        #endregion 
    }
}
