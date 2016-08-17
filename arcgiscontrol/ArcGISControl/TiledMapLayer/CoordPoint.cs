using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGISControl.TiledMapLayer
{
    internal class CoordPoint
    {
        public double X;
        public double Y;
        private double m_imode = 0.0D;
        private double m_ds = 0.0D;
        private double m_kappa = 0.0D;
        private double m_phi = 0.0D;
        private double m_omega = 0.0D;
        private double m_dz = 0.0D;
        private double m_dy = 0.0D;
        private double m_dx = 0.0D;
        private static double m_AW = 6378137.0D;
        private static double m_FW = 0.003352810664747481D;
        private static double m_AB = 6377397.1550000003D;
        private static double m_FB = 0.003342773179939979D;
        private static double m_OKKTM = 1.0D;
        private static double m_OKUTM = 0.9996D;
        private static double m_OKGTM = 0.9999D;
        private static double m_TX = 115.8D;
        private static double m_TY = -474.99000000000001D;
        private static double m_TZ = -674.11000000000001D;
        private static double m_TOMEGA = 1.16D;
        private static double m_TPHI = -2.31D;
        private static double m_TKAPPA = -1.63D;
        private static double m_TS = -6.43D;
        private static double m_TMODE = 1.0D;
        private static double m_ux0 = 0.0D;
        private static double m_uy0 = 500000.0D;
        private static double m_x0 = 500000.0D;
        private static double m_y0 = 200000.0D;
        private static double m_x1 = 600000.0D;
        private static double m_y1 = 400000.0D;
        private CoordRect[] rectArray1 = { 
                        new CoordRect(112500.0D, -50000.0D, 33500.0D, 53000.0D), 
                        new CoordRect(146000.0D, -50000.0D, 54000.0D, 58600.0D), 
                        new CoordRect(130000.0D, 44000.0D, 15000.0D, 14000.0D), 
                        new CoordRect(532500.0D, 437500.0D, 25000.0D, 25000.0D), 
                        new CoordRect(625000.0D, 412500.0D, 25000.0D, 25000.0D), 
                        new CoordRect(-12500.0D, 462500.0D, 17500.0D, 50000.0D) };

        private CoordRect[] rectArray2 = { 
                        new CoordRect(112500.0D, -50000.0D, 33500.0D, 53000.0D), 
                        new CoordRect(146000.0D, -50000.0D, 54000.0D, 58600.0D), 
                        new CoordRect(130000.0D, 44000.0D, 15000.0D, 14000.0D), 
                        new CoordRect(532500.0D, 437500.0D, 25000.0D, 25000.0D), 
                        new CoordRect(625000.0D, 412500.0D, 25000.0D, 25000.0D), 
                        new CoordRect(-12500.0D, 462500.0D, 17500.0D, 50000.0D) };

        private double[,] deltaValue1 = new double[,]{ 
                                    { 0.0D, 50000.0D }, 
                                    { 0.0D, 50000.0D }, 
                                    { 0.0D, 10000.0D }, 
                                    { -70378.0D, -136.0D }, 
                                    { -144738.0D, -2161.0D }, 
                                    { 23510.0D, -111.0D } };

        private double[,] deltaValue2 = new double[,]{ 
                                    { 0.0D, -50000.0D }, 
                                    { 0.0D, -50000.0D }, 
                                    { 0.0D, -10000.0D }, 
                                    { 70378.0D, 136.0D }, 
                                    { 144738.0D, 2161.0D }, 
                                    { -23510.0D, 111.0D } };

        public CoordPoint()
        {
            for (int i = 0; i < this.rectArray2.Length; i++)
            {
                this.rectArray2[i].x += this.deltaValue1[i, 0];
                this.rectArray2[i].y += this.deltaValue1[i, 1];
            }
        }

        public CoordPoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public CoordPoint clone()
        {
            return new CoordPoint(this.X, this.Y);
        }

        public void convertBESSEL2KTM()
        {
            GP2TM(6377397.1550000003D, 0.003342773179939979D, 600000.0D, 400000.0D, 0.9999D, 38.0D, 128.0D);
        }

        public void convertBESSEL2CONG()
        {
            GP2TM(6377397.1550000003D, 0.003342773179939979D, 500000.0D, 200000.0D, 1.0D, 38.0D,
              127.00289027777778D);
            shiftIsland(true);
        }

        public void convertBESSEL2WGS()
        {
            setParameter(115.8D, -474.99000000000001D, -674.11000000000001D, 1.16D, -2.31D, -1.63D, -6.43D,
              1.0D);
            double[] rtn = GP2WGP(this.Y, this.X, 0.0D, 6377397.1550000003D, 0.003342773179939979D);
            this.X = rtn[1];
            this.Y = rtn[0];
        }

        public void convertKTM2BESSEL()
        {
            TM2GP(6377397.1550000003D, 0.003342773179939979D, 600000.0D, 400000.0D, 0.9999D, 38.0D, 128.0D);
        }

        public void convertBESSEL2TM(double d, double e)
        {
            GP2TM(6377397.1550000003D, 0.003342773179939979D, 500000.0D, 200000.0D, 1.0D, e, d + 0.002890277777777778D);
        }

        public void convertTM2BESSEL(double d, double e)
        {
            TM2GP(6377397.1550000003D, 0.003342773179939979D, 500000.0D, 200000.0D, 1.0D, e, d + 0.002890277777777778D);
        }

        public void convertWGS2UTM(double d, double e)
        {
            setParameter(115.8D, -474.99000000000001D, -674.11000000000001D, 1.16D, -2.31D, -1.63D, -6.43D, 1.0D);
            GP2TM(6378137.0D, 0.003352810664747481D, 0.0D, 500000.0D, 0.9996D, e, d);
        }

        public void convertWGS2WTM(double d, double e)
        {
            GP2TM(6378137.0D, 0.003352810664747481D, 500000.0D, 200000.0D, 1.0D, e, d);
        }

        public void convertWGS2WKTM()
        {
            GP2TM(6378137.0D, 0.003352810664747481D, 600000.0D, 400000.0D, 0.9999D, 38.0D, 128.0D);
        }

        public void convertWGS2WCONG()
        {
            GP2TM(6378137.0D, 0.003352810664747481D, 500000.0D, 200000.0D, 1.0D, 38.0D, 127.0D);
            this.X = Math.Round(this.X * 2.5D);
            this.Y = Math.Round(this.Y * 2.5D);
        }

        public void convertUTM2WGS(double d, double e)
        {
            setParameter(115.8D, -474.99000000000001D, -674.11000000000001D, 1.16D, -2.31D, -1.63D, -6.43D, 1.0D);
            TM2GP(6378137.0D, 0.003352810664747481D, 0.0D, 500000.0D, 0.9996D, e, d);
        }

        public void convertWGS2BESSEL()
        {
            setParameter(115.8D, -474.99000000000001D, -674.11000000000001D, 1.16D, -2.31D, -1.63D, -6.43D, 1.0D);
            double[] rtn = WGP2GP(this.Y, this.X, 0.0D, 6377397.1550000003D, 0.003342773179939979D);
            this.X = rtn[1];
            this.Y = rtn[0];
        }

        public void convertCONG2BESSEL()
        {
            shiftIsland(false);
            TM2GP(6377397.1550000003D, 0.003342773179939979D, 500000.0D, 200000.0D, 1.0D, 38.0D, 127.00289027777778D);
        }

        public void convertWTM2WGS(double d, double e)
        {
            TM2GP(6378137.0D, 0.003352810664747481D, 500000.0D, 200000.0D, 1.0D, e, d);
        }

        public void convertWKTM2WGS()
        {
            TM2GP(6378137.0D, 0.003352810664747481D, 600000.0D, 400000.0D, 0.9999D, 38.0D, 128.0D);
        }

        public void convertWCONG2WGS()
        {
            this.X /= 2.5D;
            this.Y /= 2.5D;
            TM2GP(6378137.0D, 0.003352810664747481D, 500000.0D, 200000.0D, 1.0D, 38.0D, 127.0D);
        }

        private double[] WGP2GP(double a, double b, double d, double e, double h)
        {
            double[] rtn = WGP2WCTR(a, b, d);
            if (this.m_imode == 1.0D)
                rtn = TransMolod(rtn[0], rtn[1], rtn[2]);
            else
            {
                rtn = TransBursa(rtn[0], rtn[1], rtn[2]);
            }

            return CTR2GP(rtn[0], rtn[1], rtn[2], e, h);
        }

        private double[] WGP2WCTR(double a, double b, double d)
        {
            return GP2CTR(a, b, d, 6378137.0D, 0.003352810664747481D);
        }

        private double[] GP2WGP(double a, double b, double d, double e, double h)
        {
            double[] rtn = GP2CTR(a, b, d, e, h);

            if (this.m_imode == 1.0D)
                rtn = InverseMolod(rtn[0], rtn[1], rtn[2]);
            else
            {
                rtn = InverseBursa(rtn[0], rtn[1], rtn[2]);
            }

            return WCTR2WGP(rtn[0], rtn[1], rtn[2]);
        }

        private double[] GP2CTR(double a, double b, double d, double e, double h)
        {
            double[] rtn = new double[3];
            double j = 0.0D;
            double l = 0.0D;
            double o = 0.0D;
            double m = h;

            if (m > 1.0D)
                m = 1.0D / m;
            j = Math.Atan(1.0D) / 45.0D;
            l = a * j;
            j = b * j;
            m = 1.0D / m;
            m = e * (m - 1.0D) / m;
            o = (Math.Pow(e, 2.0D) - Math.Pow(m, 2.0D)) / Math.Pow(e, 2.0D);
            o = e / Math.Sqrt(1.0D - o * Math.Pow(Math.Sin(l), 2.0D));

            rtn[0] = ((o + d) * Math.Cos(l) * Math.Cos(j));
            rtn[1] = ((o + d) * Math.Cos(l) * Math.Sin(j));
            rtn[2] = ((Math.Pow(m, 2.0D) / Math.Pow(e, 2.0D) * o + d) * Math.Sin(l));

            return rtn;
        }

        private double[] InverseMolod(double a, double b, double d)
        {
            double[] rtn = new double[3];
            double e = 0.0D;
            double h = 0.0D;
            double g = 0.0D;
            e = (a - this.m_dx) * (1.0D + this.m_ds);
            h = (b - this.m_dy) * (1.0D + this.m_ds);
            g = (d - this.m_dz) * (1.0D + this.m_ds);

            rtn[0] = (1.0D / (1.0D + this.m_ds) * (e - this.m_kappa * h + this.m_phi * g));
            rtn[1] = (1.0D / (1.0D + this.m_ds) * (this.m_kappa * e + h - this.m_omega * g));
            rtn[2] = (1.0D / (1.0D + this.m_ds) * (-1.0D * this.m_phi * e + this.m_omega * h + g));

            return rtn;
        }

        private double[] InverseBursa(double a, double b, double d)
        {
            double e = a - this.m_dx;
            double h = b - this.m_dy;
            double g = d - this.m_dz;

            double[] rtn = new double[3];

            rtn[0] = (1.0D / (1.0D + this.m_ds) * (e - this.m_kappa * h + this.m_phi * g));
            rtn[1] = (1.0D / (1.0D + this.m_ds) * (this.m_kappa * e + h - this.m_omega * g));
            rtn[2] = (1.0D / (1.0D + this.m_ds) * (-1.0D * this.m_phi * e + this.m_omega * h + g));

            return rtn;
        }

        private double[] TransMolod(double a, double b, double d)
        {
            double[] rtn = new double[3];
            rtn[0] = (a + (1.0D + this.m_ds) * (this.m_kappa * b - this.m_phi * d) + this.m_dx);
            rtn[1] = (b + (1.0D + this.m_ds) * (-1.0D * this.m_kappa * a + this.m_omega * d) + this.m_dy);
            rtn[2] = (d + (1.0D + this.m_ds) * (this.m_phi * a - this.m_omega * b) + this.m_dz);

            return rtn;
        }

        private double[] TransBursa(double a, double b, double d)
        {
            double[] rtn = new double[3];
            rtn[0] = ((1.0D + this.m_ds) * (a + this.m_kappa * b - this.m_phi * d) + this.m_dx);
            rtn[1] = ((1.0D + this.m_ds) * (-1.0D * this.m_kappa * a + b + this.m_omega * d) + this.m_dy);
            rtn[2] = ((1.0D + this.m_ds) * (this.m_phi * a - this.m_omega * b + d) + this.m_dz);

            return rtn;
        }

        private double[] WCTR2WGP(double a, double b, double d)
        {
            return CTR2GP(a, b, d, 6378137.0D, 0.003352810664747481D);
        }

        private double[] CTR2GP(double a, double b, double d, double e, double h)
        {
            double m = h;
            double w = 0.0D;
            double g = 0.0D;
            double o = 0.0D;
            double D = 0.0D;
            double A = 0.0D;
            double u = 0.0D;
            double l = 0.0D;
            double j = 0.0D;

            if (m > 1.0D)
            {
                m = 1.0D / m;
            }
            g = Math.Atan(1.0D) / 45.0D;
            m = 1.0D / m;
            o = e * (m - 1.0D) / m;
            D = (Math.Pow(e, 2.0D) - Math.Pow(o, 2.0D)) / Math.Pow(e, 2.0D);
            m = Math.Atan(b / a);
            A = Math.Sqrt(a * a + b * b);
            u = e;
            b = 0.0D;
            do
            {
                b += 1.0D;
                w = Math.Pow(Math.Pow(o, 2.0D) / Math.Pow(e, 2.0D) * u + w, 2.0D) -
                  Math.Pow(d, 2.0D);
                w = d / Math.Sqrt(w);
                l = Math.Atan(w);
                if (Math.Abs(l - j) < 1.0E-018D)
                    break;
                u = e / Math.Sqrt(1.0D - D * Math.Pow(Math.Sin(l), 2.0D));
                w = A / Math.Cos(l) - u;
                j = l;
            } while (b <= 30.0D);

            double[] rtn = new double[2];
            rtn[0] = (l / g);
            rtn[1] = (m / g);
            if (a < 0.0D)
                rtn[1] = (180.0D + rtn[1]);
            if (rtn[1] < 0.0D)
                rtn[1] = (360.0D + rtn[1]);
            return rtn;
        }

        private void GP2TM(double d, double e, double h, double g, double j, double l, double m)
        {
            double a = this.Y;
            double b = this.X;
            double w = e;
            double A = 0.0D;
            double o = 0.0D;
            double D = 0.0D;
            double u = 0.0D;
            double z = 0.0D;
            double G = 0.0D;
            double E = 0.0D;
            double I = 0.0D;
            double J = 0.0D;
            double L = 0.0D;
            double M = 0.0D;
            double H = 0.0D;
            double B = g;

            if (w > 1.0D)
                w = 1.0D / w;
            A = Math.Atan(1.0D) / 45.0D;
            o = a * A;
            D = b * A;
            u = l * A;
            A = m * A;
            w = 1.0D / w;
            z = d * (w - 1.0D) / w;
            G = (Math.Pow(d, 2.0D) - Math.Pow(z, 2.0D)) / Math.Pow(d, 2.0D);
            w = (Math.Pow(d, 2.0D) - Math.Pow(z, 2.0D)) / Math.Pow(z, 2.0D);
            z = (d - z) / (d + z);
            E = d * (
              1.0D - z + 5.0D * (Math.Pow(z, 2.0D) - Math.Pow(z, 3.0D)) / 4.0D + 81.0D * (
              Math.Pow(z, 4.0D) - Math.Pow(z, 5.0D)) / 64.0D);
            I = 3.0D *
              d * (
              z - Math.Pow(z, 2.0D) + 7.0D * (
              Math.Pow(z, 3.0D) - Math.Pow(z, 4.0D)) / 8.0D + 55.0D *
              Math.Pow(z, 5.0D) / 64.0D) / 2.0D;
            J = 15.0D *
              d * (
              Math.Pow(z, 2.0D) - Math.Pow(z, 3.0D) + 3.0D * (Math.Pow(z, 4.0D) -
              Math.Pow(z, 5.0D)) / 4.0D) / 16.0D;
            L = 35.0D *
              d * (
              Math.Pow(z, 3.0D) - Math.Pow(z, 4.0D) + 11.0D * Math.Pow(z, 5.0D) / 16.0D) /
              48.0D;
            M = 315.0D * d * (Math.Pow(z, 4.0D) - Math.Pow(z, 5.0D)) / 512.0D;
            D -= A;
            u = E * u - I * Math.Sin(2.0D * u) + J * Math.Sin(4.0D * u) - L *
              Math.Sin(6.0D * u) + M * Math.Sin(8.0D * u);
            z = u * j;
            H = Math.Sin(o);
            u = Math.Cos(o);
            A = H / u;
            w *= Math.Pow(u, 2.0D);
            G = d / Math.Sqrt(1.0D - G * Math.Pow(Math.Sin(o), 2.0D));
            o = E * o - I * Math.Sin(2.0D * o) + J * Math.Sin(4.0D * o) - L *
              Math.Sin(6.0D * o) + M * Math.Sin(8.0D * o);
            o *= j;
            E = G * H * u * j / 2.0D;
            I = G * H * Math.Pow(u, 3.0D) * j * (
              5.0D - Math.Pow(A, 2.0D) + 9.0D * w + 4.0D * Math.Pow(w, 2.0D)) / 24.0D;
            J = G *
              H *
              Math.Pow(u, 5.0D) *
              j * (
              61.0D - 58.0D * Math.Pow(A, 2.0D) + Math.Pow(A, 4.0D) + 270.0D * w -
              330.0D * Math.Pow(A, 2.0D) * w + 445.0D * Math.Pow(w, 2.0D) +
              324.0D * Math.Pow(w, 3.0D) - 680.0D * Math.Pow(A, 2.0D) *
              Math.Pow(w, 2.0D) + 88.0D * Math.Pow(w, 4.0D) - 600.0D *
              Math.Pow(A, 2.0D) * Math.Pow(w, 3.0D) - 192.0D *
              Math.Pow(A, 2.0D) * Math.Pow(w, 4.0D)) / 720.0D;
            H = G *
              H *
              Math.Pow(u, 7.0D) *
              j * (
              1385.0D - 3111.0D * Math.Pow(A, 2.0D) + 543.0D * Math.Pow(A, 4.0D) -
              Math.Pow(A, 6.0D)) / 40320.0D;
            o = o + Math.Pow(D, 2.0D) * E + Math.Pow(D, 4.0D) * I + Math.Pow(D, 6.0D) *
              J + Math.Pow(D, 8.0D) * H;
            this.Y = (o - z + h);
            o = G * u * j;
            z = G * Math.Pow(u, 3.0D) * j * (1.0D - Math.Pow(A, 2.0D) + w) / 6.0D;
            w = G *
              Math.Pow(u, 5.0D) *
              j * (
              5.0D - 18.0D * Math.Pow(A, 2.0D) + Math.Pow(A, 4.0D) + 14.0D * w - 58.0D *
              Math.Pow(A, 2.0D) * w + 13.0D * Math.Pow(w, 2.0D) + 4.0D *
              Math.Pow(w, 3.0D) - 64.0D * Math.Pow(A, 2.0D) *
              Math.Pow(w, 2.0D) - 25.0D * Math.Pow(A, 2.0D) *
              Math.Pow(w, 3.0D)) / 120.0D;
            u = G *
              Math.Pow(u, 7.0D) *
              j * (
              61.0D - 479.0D * Math.Pow(A, 2.0D) + 179.0D * Math.Pow(A, 4.0D) -
              Math.Pow(A, 6.0D)) / 5040.0D;
            this.X =
              (B + D * o + Math.Pow(D, 3.0D) * z + Math.Pow(D, 5.0D) * w +
              Math.Pow(D, 7.0D) * u);
        }

        private void TM2GP(double d, double e, double h, double g, double j, double l, double m)
        {
            double u = e;
            double A = 0.0D;
            double w = 0.0D;
            double o = 0.0D;
            double D = 0.0D;
            double B = 0.0D;
            double z = 0.0D;
            double G = 0.0D;
            double E = 0.0D;
            double I = 0.0D;
            double J = 0.0D;
            double L = 0.0D;
            double M = 0.0D;
            double H = 0.0D;
            double a = this.Y;
            double b = this.X;

            if (u > 1.0D)
                u = 1.0D / u;
            A = g;
            w = Math.Atan(1.0D) / 45.0D;
            o = l * w;
            D = m * w;
            u = 1.0D / u;
            B = d * (u - 1.0D) / u;
            z = (Math.Pow(d, 2.0D) - Math.Pow(B, 2.0D)) / Math.Pow(d, 2.0D);
            u = (Math.Pow(d, 2.0D) - Math.Pow(B, 2.0D)) / Math.Pow(B, 2.0D);
            B = (d - B) / (d + B);
            G = d * (
              1.0D - B + 5.0D * (Math.Pow(B, 2.0D) - Math.Pow(B, 3.0D)) / 4.0D + 81.0D * (
              Math.Pow(B, 4.0D) - Math.Pow(B, 5.0D)) / 64.0D);
            E = 3.0D *
              d * (
              B - Math.Pow(B, 2.0D) + 7.0D * (
              Math.Pow(B, 3.0D) - Math.Pow(B, 4.0D)) / 8.0D + 55.0D *
              Math.Pow(B, 5.0D) / 64.0D) / 2.0D;
            I = 15.0D *
              d * (
              Math.Pow(B, 2.0D) - Math.Pow(B, 3.0D) + 3.0D * (Math.Pow(B, 4.0D) -
              Math.Pow(B, 5.0D)) / 4.0D) / 16.0D;
            J = 35.0D *
              d * (
              Math.Pow(B, 3.0D) - Math.Pow(B, 4.0D) + 11.0D * Math.Pow(B, 5.0D) / 16.0D) /
              48.0D;
            L = 315.0D * d * (Math.Pow(B, 4.0D) - Math.Pow(B, 5.0D)) / 512.0D;
            o = G * o - E * Math.Sin(2.0D * o) + I * Math.Sin(4.0D * o) - J *
              Math.Sin(6.0D * o) + L * Math.Sin(8.0D * o);
            o *= j;
            o = a + o - h;
            M = o / j;
            H = d * (1.0D - z) /
              Math.Pow(Math.Sqrt(1.0D - z * Math.Pow(Math.Sin(0.0D), 2.0D)), 3.0D);
            o = M / H;
            for (a = 1.0D; a <= 5.0D; a += 1.0D)
            {
                B = G * o - E * Math.Sin(2.0D * o) + I * Math.Sin(4.0D * o) - J *
                  Math.Sin(6.0D * o) + L * Math.Sin(8.0D * o);
                H = d * (
                  1.0D - z) /
                  Math.Pow(Math.Sqrt(1.0D - z * Math.Pow(Math.Sin(o), 2.0D)),
                  3.0D);
                o += (M - B) / H;
            }
            H = d * (1.0D - z) /
              Math.Pow(Math.Sqrt(1.0D - z * Math.Pow(Math.Sin(o), 2.0D)), 3.0D);
            G = d / Math.Sqrt(1.0D - z * Math.Pow(Math.Sin(o), 2.0D));
            B = Math.Sin(o);
            z = Math.Cos(o);
            E = B / z;
            u *= Math.Pow(z, 2.0D);
            A = b - A;
            B = E / (2.0D * H * G * Math.Pow(j, 2.0D));
            I = E * (
              5.0D + 3.0D * Math.Pow(E, 2.0D) + u - 4.0D * Math.Pow(u, 2.0D) - 9.0D *
              Math.Pow(E, 2.0D) * u) / (
              24.0D * H * Math.Pow(G, 3.0D) * Math.Pow(j, 4.0D));
            J = E * (
              61.0D + 90.0D * Math.Pow(E, 2.0D) + 46.0D * u + 45.0D * Math.Pow(E, 4.0D) -
              252.0D * Math.Pow(E, 2.0D) * u - 3.0D * Math.Pow(u, 2.0D) +
              100.0D * Math.Pow(u, 3.0D) - 66.0D * Math.Pow(E, 2.0D) *
              Math.Pow(u, 2.0D) - 90.0D * Math.Pow(E, 4.0D) * u + 88.0D *
              Math.Pow(u, 4.0D) + 225.0D * Math.Pow(E, 4.0D) *
              Math.Pow(u, 2.0D) + 84.0D * Math.Pow(E, 2.0D) *
              Math.Pow(u, 3.0D) - 192.0D * Math.Pow(E, 2.0D) *
              Math.Pow(u, 4.0D)) / (
              720.0D * H * Math.Pow(G, 5.0D) * Math.Pow(j, 6.0D));
            H = E * (
              1385.0D + 3633.0D * Math.Pow(E, 2.0D) + 4095.0D * Math.Pow(E, 4.0D) + 1575.0D *
              Math.Pow(E, 6.0D)) / (
              40320.0D * H * Math.Pow(G, 7.0D) * Math.Pow(j, 8.0D));
            o = o - Math.Pow(A, 2.0D) * B + Math.Pow(A, 4.0D) * I - Math.Pow(A, 6.0D) *
              J + Math.Pow(A, 8.0D) * H;
            B = 1.0D / (G * z * j);
            H = (1.0D + 2.0D * Math.Pow(E, 2.0D) + u) / (
              6.0D * Math.Pow(G, 3.0D) * z * Math.Pow(j, 3.0D));
            u = (5.0D + 6.0D * u + 28.0D * Math.Pow(E, 2.0D) - 3.0D * Math.Pow(u, 2.0D) + 8.0D *
              Math.Pow(E, 2.0D) * u + 24.0D * Math.Pow(E, 4.0D) - 4.0D *
              Math.Pow(u, 3.0D) + 4.0D * Math.Pow(E, 2.0D) * Math.Pow(u, 2.0D) + 24.0D *
              Math.Pow(E, 2.0D) * Math.Pow(u, 3.0D)) / (
              120.0D * Math.Pow(G, 5.0D) * z * Math.Pow(j, 5.0D));
            z = (61.0D + 662.0D * Math.Pow(E, 2.0D) + 1320.0D * Math.Pow(E, 4.0D) + 720.0D *
              Math.Pow(E, 6.0D)) / (
              5040.0D * Math.Pow(G, 7.0D) * z * Math.Pow(j, 7.0D));
            A = A * B - Math.Pow(A, 3.0D) * H + Math.Pow(A, 5.0D) * u -
              Math.Pow(A, 7.0D) * z;
            D += A;

            this.X = (D / w);
            this.Y = (o / w);
        }

        private void setParameter(double a, double b, double d, double e, double h, double g, double j, double l)
        {
            double m = Math.Atan(1.0D) / 45.0D;
            this.m_dx = a;
            this.m_dy = b;
            this.m_dz = d;
            this.m_omega = (e / 3600.0D * m);
            this.m_phi = (h / 3600.0D * m);
            this.m_kappa = (g / 3600.0D * m);
            this.m_ds = (j * 1.0E-006D);
            this.m_imode = l;
        }

        private void shiftIsland(bool d)
        {
            double e = 0.0D;
            double h = 0.0D;
            double y;
            double x;

            if (d)
            {
                for (int i = 0; i < this.rectArray1.Length; i++)
                {
                    if ((this.X - this.rectArray1[i].x < 0.0D) ||
                      (this.X - this.rectArray1[i].x > this.rectArray1[i].w) ||
                      (this.Y - this.rectArray1[i].y < 0.0D) ||
                      (this.Y - this.rectArray1[i].y > this.rectArray1[i].h)) continue;
                    e += this.deltaValue1[i, 0];
                    h += this.deltaValue1[i, 1];
                    break;
                }

                x = (int)((this.X + e) * 2.5D + 0.5D);
                y = (int)((this.Y + h) * 2.5D + 0.5D);
            }
            else
            {
                x = this.X / 2.5D;
                y = this.Y / 2.5D;
                for (int i = 0; i < this.rectArray2.Length; i++)
                {
                    if ((x - this.rectArray2[i].x < 0.0D) ||
                      (x - this.rectArray2[i].x > this.rectArray2[i].w) ||
                      (y - this.rectArray2[i].y < 0.0D) ||
                      (y - this.rectArray2[i].y > this.rectArray2[i].h)) continue;
                    x += this.deltaValue2[i, 0];
                    y += this.deltaValue2[i, 1];
                    break;
                }

            }

            this.X = x;
            this.Y = y;
        }

        private class CoordRect
        {
            public double x;
            public double y;
            public double w;
            public double h;

            public CoordRect(double x, double y, double w, double h)
            {
                this.x = x;
                this.y = y;
                this.w = w;
                this.h = h;
            }
        }
    }
}
