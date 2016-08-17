using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Commons.Maps
{
    public class WaferTileCreator
    {
        public readonly List<double> Magnifications = new List<double>
            {
                0.000030517578125, 
                0.000061035156250,
                0.000122070312500, 
                0.000244140625000, 
                0.000488281250000, 
                0.000976562500000, 
                0.001953125000000, 
                0.003906250000000, 
                0.007812500000000, 
                0.015625000000000, 
                0.031250000000000, 
                0.062500000000000, 
                0.125000000000000, 
                0.250000000000000, 
                0.500000000000000, 
                1.000000000000000 
            };

        #region Properties

        private int ZoomLevelOffSet = -2;

        public double Cell_Width { get; set; }
        public double Cell_Height { get; set; }

        public double Bank_Cell_Col { get; set; }
        public double Bank_Cell_Row { get; set; }

        public double Die_Bank_Col { get; set; }
        public double Die_Bank_Row { get; set; }

        public double Wafer_Die_Col { get; set; }
        public double Wafer_Die_Row { get; set; }

        /// <summary>
        /// 타일 크기
        /// </summary>
        public int TileSize { get; set; }

        /// <summary>
        /// 화면 표출 크기 제한. 이 크기보다 작은 오브젝트(Cell, Bank, Die)는 그려지지 않음.
        /// </summary>
        public int DisplaySizeLimit { get; set; }

        //public Brush CellBrush { get; set; }
        //public Brush BankBrush { get; set; }
        //public Brush DieBrush { get; set; }
        //public Brush WaferBrush { get; set; }

        #endregion // Properties

        public static WaferTileCreator Instance
        {
            get
            {
                return Nested<WaferTileCreator>.Instance;
            }
        }


        private WaferTileCreator()
        {
            this.Cell_Width = 64;
            this.Cell_Height = 64;

            this.Bank_Cell_Col = 16383; // 0x3FFF
            this.Bank_Cell_Row = 1232; // 0x133 * 4

            this.Die_Bank_Col = 2;
            this.Die_Bank_Row = 4;

            this.Wafer_Die_Col = 20;
            this.Wafer_Die_Row = 100;

            this.TileSize = 256;

            this.DisplaySizeLimit = 4;

            //this.CellBrush = Brushes.Blue;
            //this.BankBrush = Brushes.Orange;
            //this.DieBrush = Brushes.Red;
            //this.WaferBrush = Brushes.Green;
        }

        public MemoryStream GetTile(int aZLevel, int aRow, int aCol)
        {
            var zl = aZLevel + this.ZoomLevelOffSet;
            if (zl  < 1 || zl > this.Magnifications.Count) return null;

            double cellWidth, cellHeight, bankWidth, bankHeight, dieWidth, dieHeight, waferWidth, waferHeight;
             
            double magnification = this.Magnifications[zl - 1]; // zoom level에 따른 배율

            // zoom level과 내부 오브젝트의 개수가 적용된 오브젝트들의 크기
            cellWidth = this.Cell_Width * magnification;
            cellHeight = this.Cell_Height * magnification;
            bankWidth = cellWidth * this.Bank_Cell_Col;
            bankHeight = cellHeight * this.Bank_Cell_Row;
            dieWidth = bankWidth * this.Die_Bank_Col;
            dieHeight = bankHeight * this.Die_Bank_Row;
            waferWidth = dieWidth * this.Wafer_Die_Col;
            waferHeight = dieHeight * this.Wafer_Die_Row;

            var dieTotalCol = this.Wafer_Die_Col;
            var dieTotalRow = this.Wafer_Die_Row;
            var bankTotalCol = dieTotalCol * this.Die_Bank_Col;
            var bankTotalRow = dieTotalRow * this.Die_Bank_Row;
            var cellTotalCol = bankTotalCol * this.Bank_Cell_Col;
            var cellTotalRow = bankTotalRow * this.Bank_Cell_Row;

            long sx, sy, ex, ey; // 타일 시작 위치, 타일 끝 위치
            sx = this.TileSize * aCol;
            sy = this.TileSize * aRow;
            ex = sx + this.TileSize - 1;
            ey = sy + this.TileSize - 1;

            if (sx >= waferWidth || sy >= waferHeight) return null;

            var bitmap = new Bitmap(this.TileSize, this.TileSize);
            var graphic = Graphics.FromImage(bitmap);

            //var cellPen = new Pen(this.CellBrush);
            //var bankPen = new Pen(this.BankBrush, 1.5f);
            //var diePen = new Pen(this.DieBrush, 2);
            //var waferPen = new Pen(this.WaferBrush, 2.5f);

            var cellPen = new Pen(Brushes.Blue);
            var bankPen = new Pen(Brushes.Orange, 1.5f);
            var diePen = new Pen(Brushes.Red, 2);
            var waferPen = new Pen(Brushes.Green, 2.5f);
            
            long isx, isy; // 타일에 포함될 오브젝트의 시작 인덱스
            long iex, iey; // 타일에 포함될 오브젝트의 끝 인덱스
            float left, top, right, bottom; // 오브젝트의 그리기 좌표

            long fx, fy; // for loop 변수

            // draw cell
            if (cellWidth > this.DisplaySizeLimit && cellHeight > this.DisplaySizeLimit)
            {
                isx = (long)Math.Truncate(sx / cellWidth);
                isy = (long)Math.Truncate(sy / cellHeight);

                iex = (long)Math.Ceiling(ex / cellWidth);
                iey = (long)Math.Ceiling(ey / cellHeight);

                for (fy = isy; fy <= iey; fy++)
                {
                    if (fy >= cellTotalRow) continue;

                    for (fx = isx; fx <= iex; fx++)
                    {
                        if (fx >= cellTotalCol) continue;

                        left = (long)Math.Round(fx * cellWidth) - sx;
                        right = (long)Math.Round((fx + 1) * cellWidth) - sx;
                        top = (long)Math.Round(fy * cellHeight) - sy;
                        bottom = (long)Math.Round((fy + 1) * cellWidth) - sy;

                        graphic.DrawLine(cellPen, left < 0 ? 0 : left, top, right > this.TileSize ? this.TileSize : right, top);
                        graphic.DrawLine(cellPen, left < 0 ? 0 : left, bottom, right > this.TileSize ? this.TileSize : right, bottom);
                        graphic.DrawLine(cellPen, left, top < 0 ? 0 : top, left, bottom > this.TileSize ? this.TileSize : bottom);
                        graphic.DrawLine(cellPen, right, top < 0 ? 0 : top, right, bottom > this.TileSize ? this.TileSize : bottom);
                    }
                }
            }

            // draw bank
            if (bankWidth > this.DisplaySizeLimit && bankHeight > this.DisplaySizeLimit)
            {
                isx = (long)Math.Truncate(sx / bankWidth);
                isy = (long)Math.Truncate(sy / bankHeight);

                iex = (long)Math.Ceiling(ex / bankWidth);
                iey = (long)Math.Ceiling(ey / bankHeight);

                for (fy = isy; fy <= iey; fy++)
                {
                    if (fy >= bankTotalRow) continue;

                    for (fx = isx; fx <= iex; fx++)
                    {
                        if (fx >= bankTotalCol) continue;

                        left = (long)Math.Round(fx * bankWidth) - sx;
                        right = (long)Math.Round((fx + 1) * bankWidth) - sx - 1;
                        top = (long)Math.Round(fy * bankHeight) - sy;
                        bottom = (long)Math.Round((fy + 1) * bankHeight) - sy - 1;

                        graphic.DrawLine(bankPen, left < 0 ? 0 : left, top, right > this.TileSize ? this.TileSize : right, top);
                        graphic.DrawLine(bankPen, left < 0 ? 0 : left, bottom, right > this.TileSize ? this.TileSize : right, bottom);
                        graphic.DrawLine(bankPen, left, top < 0 ? 0 : top, left, bottom > this.TileSize ? this.TileSize : bottom);
                        graphic.DrawLine(bankPen, right, top < 0 ? 0 : top, right, bottom > this.TileSize ? this.TileSize : bottom);
                    }
                }
            }

            // draw die
            if (dieWidth > this.DisplaySizeLimit && dieHeight > this.DisplaySizeLimit)
            {
                isx = (long)Math.Truncate(sx / dieWidth);
                isy = (long)Math.Truncate(sy / dieHeight);

                iex = (long)Math.Ceiling(ex / dieWidth);
                iey = (long)Math.Ceiling(ey / dieHeight);

                for (fy = isy; fy <= iey; fy++)
                {
                    if (fy >= dieTotalRow) continue;

                    for (fx = isx; fx <= iex; fx++)
                    {
                        if (fx >= dieTotalCol) continue;

                        left = (long)Math.Round(fx * dieWidth) - sx;
                        right = (long)Math.Round((fx + 1) * dieWidth) - sx;
                        top = (long)Math.Round(fy * dieHeight) - sy;
                        bottom = (long)Math.Round((fy + 1) * dieHeight) - sy;

                        graphic.DrawLine(diePen, left < 0 ? 0 : left, top, right > this.TileSize ? this.TileSize : right, top);
                        graphic.DrawLine(diePen, left < 0 ? 0 : left, bottom, right > this.TileSize ? this.TileSize : right, bottom);
                        graphic.DrawLine(diePen, left, top < 0 ? 0 : top, left, bottom > this.TileSize ? this.TileSize : bottom);
                        graphic.DrawLine(diePen, right, top < 0 ? 0 : top, right, bottom > this.TileSize ? this.TileSize : bottom);
                    }
                }
            }

            // draw wafer
            if (waferWidth > this.DisplaySizeLimit && waferHeight > this.DisplaySizeLimit)
            {
                left = -sx;
                right = (long)Math.Round(waferWidth) - sx;
                top = -sy;
                bottom = (long)Math.Round(waferHeight) - sy;

                graphic.DrawLine(waferPen, left < 0 ? 0 : left, top, right > this.TileSize ? this.TileSize : right, top);
                graphic.DrawLine(waferPen, left < 0 ? 0 : left, bottom, right > this.TileSize ? this.TileSize : right, bottom);
                graphic.DrawLine(waferPen, left, top < 0 ? 0 : top, left, bottom > this.TileSize ? this.TileSize : bottom);
                graphic.DrawLine(waferPen, right, top < 0 ? 0 : top, right, bottom > this.TileSize ? this.TileSize : bottom);
            }

            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return ms;
        }
    }
}
