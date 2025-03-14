﻿using md2visio.struc.graph;
using Microsoft.Office.Interop.Visio;
using System.Drawing;
using System.Text.RegularExpressions;
using Drawing = System.Drawing;

namespace md2visio.vsdx
{
    internal class VShapeDrawer
    {
        public static string VSSX = "md2visio.vssx";
        public Shape EmptyShape { get; }
        protected Application visioApp;
        protected Page visioPage;        
        protected Document stencilDoc;

        public VShapeDrawer(Application visioApp) {
            this.visioApp = visioApp;
            visioPage = visioApp.ActivePage;
            stencilDoc = visioApp.Documents.OpenEx(VSSX, (short)VisOpenSaveArgs.visOpenDocked);            
            EmptyShape = visioPage.DrawRectangle(0, 0, 0, 0);
        }    

        public Master? GetMaster(string vssx, string name)
        {
            stencilDoc = visioApp.Documents.OpenEx(vssx, (short)VisOpenSaveArgs.visOpenDocked);            
            return FindMaster(name);
        }

        public Master? GetMaster(string name)
        {
            return FindMaster(name);   
        }
        public Master? FindMaster(string name)
        {
            foreach (Master master in stencilDoc.Masters)
            {
                if (master.Name == name)
                {
                    return master;
                }
            }
            return null;
        }

        public Shape DropText(string text, SizeF paddingMM, double pinx=0, double piny=0)
        {
            Shape shape = visioPage.Drop(GetMaster("[]"), pinx, piny);
            shape.CellsU["LinePattern"].FormulaU = "=0";
            shape.CellsU["FillPattern"].FormulaU = "=0";
            shape.Text = text;
            AdjustSize(shape, paddingMM);

            return shape;
        }

        public Shape DropText(string text, double pinx = 0, double piny = 0)
        {
            return DropText(text, new SizeF(0, 0), pinx, piny);
        }

        public void Rotate(Shape shape, double rad, bool clockwise = true)
        {
            rad = rad % (2 * double.Pi);
            if (rad < double.Pi) rad = (clockwise ? -1 : 1) * rad;
            else if (rad > double.Pi) rad = (clockwise ? 1 : -1) * (2 * double.Pi - rad);

            SetShapeSheet(shape, "Angle", $"{rad} rad");
        }

        public void AdjustSize(Shape shape, SizeF paddingMM)
        {
            if (string.IsNullOrEmpty(shape.Text)) return;

            double fontSizeMM = FontSize(shape, "mm");
            string fontName = FontName(visioApp, shape);
            SizeF sizeF = MeasureTextSizeMM(shape.Text, fontName, fontSizeMM);

            double leftMargin = ShapeSheet(shape, "LeftMargin", "mm"); 
            double rightMargin = ShapeSheet(shape, "LeftMargin", "mm");
            double topMargin = ShapeSheet(shape, "TopMargin", "mm");
            double bottomMargin = ShapeSheet(shape, "BottomMargin", "mm");
            shape.CellsU["Width"].FormulaU = $"={sizeF.Width + leftMargin + rightMargin + paddingMM.Width*2} mm";
            shape.CellsU["Height"].FormulaU = $"={sizeF.Height + topMargin + bottomMargin + paddingMM.Height*2} mm"; 
        }

        public void AdjustSize(Shape shape)
        {
            AdjustSize(shape, new SizeF(0, 0));
        }
        
        public static void MoveTo(Shape shape, double pinx, double piny)
        {
            MoveTo(shape, pinx.ToString(), piny.ToString());
        }

        public static void MoveTo(Shape shape, string pinx, string piny)
        {
            shape.CellsU["PinX"].FormulaU = pinx;
            shape.CellsU["PinY"].FormulaU = piny;
        }

        public static void MoveTo(Shape? shape, PointF pos)
        {
            if (shape == null) return;

            shape.CellsU["PinX"].FormulaU = pos.X.ToString();
            shape.CellsU["PinY"].FormulaU = pos.Y.ToString();
        }

        public static void SetShapeSheet(Shape shape, string propName, string FormulaU)
        {
            shape.CellsU[propName].FormulaU = FormulaU;
        }

        public static double ShapeSheetIU(Shape shape, string propName)
        {
            return shape.CellsU[propName].ResultIU; // Result Internal Unit
        }

        public static double ShapeSheet(Shape shape, string propName, string unit)
        {
            string sval = shape.CellsU[propName].ResultStr[unit];
            return Convert.ToDouble(Regex.Replace(sval, unit + "| ", ""));
        }

        public static double VisioUnit2MM(Shape shape)
        {
            double vunit = FontSize(shape);
            if (vunit == 0) return 0;

            double mm = FontSize(shape, "mm");
            return mm / vunit;
        }

        public static double MM2VisioUnit(Shape shape)
        {
            double v2mm = VisioUnit2MM(shape);
            if (v2mm != 0) return 1 / v2mm;

            return 0;
        }

        public (bool success, string unit, double unitVal) UnitValue(string? unitStr)
        {
            bool success = false;
            string uname = string.Empty;
            double result = 0;

            if (unitStr != null)
            {
                unitStr = unitStr.Trim().ToLower();
                if (unitStr.EndsWith("pt")) uname = "pt";
                else if (unitStr.EndsWith("px")) uname = "px";
                else if (unitStr.EndsWith("mm")) uname = "mm";

                if (uname.Length > 0) unitStr = unitStr.Substring(0, unitStr.Length-uname.Length);
                success = double.TryParse(unitStr, out result);
            }

            return (success, uname, result);
        }

        public static double FontSize(Shape shape, string unit)
        {
            string sval = shape.CellsU["Char.Size"].ResultStr[unit]; // pt, mm
            return Convert.ToDouble(Regex.Replace(sval, unit + "| ", ""));
        }

        public static double FontSize(Shape shape)
        {
            return shape.CellsU["Char.Size"].ResultIU; // visio内部单位drawing units（Result Internal Unit）
        }
        public static string FontName(Application visioApp, Shape shape)
        {
            double fontId = shape.CellsU["Char.Font"].ResultIU;
            return visioApp.ActiveDocument.Fonts[fontId].Name;
        }

        public static double Width(Shape shape)
        {
            return shape.CellsU["Width"].ResultIU;
        }

        public static void SetWidth(Shape shape, string width)
        {
            SetShapeSheet(shape, "Width", width);
        }

        public static double Height(Shape shape)
        {
            return shape.CellsU["Height"].ResultIU;
        }

        public static void SetHeight(Shape shape, string height)
        {
            SetShapeSheet(shape, "Height", height);
        }

        public static double LeftMargin(Shape shape)
        {
            return shape.CellsU["LeftMargin"].ResultIU;
        }

        public static double RightMargin(Shape shape)
        {
            return shape.CellsU["RightMargin"].ResultIU;
        }

        public static double TopMargin(Shape shape)
        {
            return shape.CellsU["TopMargin"].ResultIU;
        }

        public static double BottomMargin(Shape shape)
        {
            return shape.CellsU["BottomMargin"].ResultIU;
        }

        public static double PinX(Shape shape)
        {
            return shape.CellsU["PinX"].ResultIU;
        }

        public static double PinY(Shape shape)
        {
            return shape.CellsU["PinY"].ResultIU;
        }

        public static void AlignRight(Shape shape, double right)
        {
            double pinx = right - ShapeSheetIU(shape, "Width") / 2;
            MoveTo(shape, pinx, ShapeSheetIU(shape, "PinY"));
        }

        public static void AlignBottom(Shape shape, double bottom)
        {
            double piny = bottom + ShapeSheetIU(shape, "Height") / 2;
            MoveTo(shape, ShapeSheetIU(shape, "PinX"), piny);
        }

        public static void AlignRightBottom(Shape shape, double right, double bottom)
        {
            AlignRight(shape, right);
            AlignBottom(shape, bottom);
        }

        public static void AlignLeft(Shape shape, double left)
        {
            double pinx = left + ShapeSheetIU(shape, "Width") / 2;
            MoveTo(shape, pinx, ShapeSheetIU(shape, "PinY"));
        }

        public static void AlignTop(Shape shape, double top)
        {
            double piny = top - ShapeSheetIU(shape, "Height") / 2;
            MoveTo(shape, ShapeSheetIU(shape, "PinX"), piny);
        }

        public static void AlignLeftTop(Shape shape, double left, double top)
        {
            AlignLeft(shape, left);
            AlignTop(shape, top);
        }

        public static void SetFillForegnd(Shape shape, Tuple<int,int,int> color)
        {
            SetShapeSheet(shape, "FillPattern", "1");
            SetShapeSheet(shape, "FillForegnd", $"THEMEGUARD(RGB({color.Item1},{color.Item2},{color.Item3}))");
        }

        public static void SetRounding(Shape shape, string rounding)
        {
            SetShapeSheet(shape, "Rounding", rounding);
        }

        public static double Pt2MM()
        {
            // 1 pt = 1/72 英寸, 1 英寸= 25.4 mm。
            return 1 / 72f * 25.4;
        }

        public static double MM2Pt()
        {
            double pt2mm = Pt2MM();
            if (pt2mm != 0) return 1 / pt2mm;

            return 0;
        }

        public static double MM2Pix()
        {
#pragma warning disable CA1416
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                // 每英寸有25.4毫米
                return graphics.DpiX / 25.4f;
            }
        }

        public static double Pix2MM()
        {
            double mm2pix = MM2Pix();
            if (mm2pix != 0) return 1 / mm2pix;

            return 0;
        }

        public static SizeF MeasureTextSizeMM(string text, string fontName, double fontSizeMM)
        {
#pragma warning disable CA1416
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                SizeF textSizePixels = MeasureTextSizePix(text, fontName, fontSizeMM);
                float widthMM = textSizePixels.Width * 25.4f / graphics.DpiX;
                float heightMM = textSizePixels.Height * 25.4f / graphics.DpiY;

                return new SizeF(widthMM, heightMM);
            }
        }

        public static SizeF MeasureTextSizePix(string text, string fontName, double fontSizeMM)
        {
#pragma warning disable CA1416
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                float fontSizePt = (float) (fontSizeMM * MM2Pt());
                using (Drawing.Font font = new Drawing.Font(fontName, fontSizePt))
                {
                    SizeF textSizePixels = graphics.MeasureString(string.IsNullOrEmpty(text) ? " " : text, font);
                    return textSizePixels;
                }
            }
        }

    }   


    
}
