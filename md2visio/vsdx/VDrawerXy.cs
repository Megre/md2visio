using md2visio.struc.figure;
using md2visio.struc.xy;
using md2visio.vsdx.@base;
using md2visio.vsdx.tool;
using Microsoft.Office.Interop.Visio;
using System.Drawing;

namespace md2visio.vsdx
{
    internal class VDrawerXy(XyChart figure, Application visioApp) : 
        VFigureDrawer<XyChart>(figure, visioApp)
    {
        public double ChartWidth { get; set; }
        public double ChartHeight { get; set; }
        public bool Vertical { get => figure.IsVertical; }
        public List<string> XTicks { get; set; } = [];
        public List<string> YTicks { get; set; } = [];

        List<VShapeBoundary> xBoundList = new ();
        List<VShapeBoundary> yBoundList = new ();
        List<string> colorList = figure.Config.GetStringList("config.xyChart.plotColorPalette");
        int colorIndex = 0;
        string xShowLabel = "true", xShowTitle = "true";
        string yShowLabel = "true", yShowTitle = "true";
        string titleColor = "#000000";

        public override void Draw()
        {            
            figure.Config.GetString("config.xyChart.xAxis.showLabel", out xShowLabel);
            figure.Config.GetString("config.xyChart.xAxis.showTitle", out xShowTitle);
            figure.Config.GetString("config.xyChart.yAxis.showLabel", out yShowLabel);
            figure.Config.GetString("config.xyChart.yAxis.showTitle", out yShowTitle);
            figure.Config.GetString("config.themeVariables.xyChart.titleColor", out titleColor);

            if (Vertical) DrawXAxis(); else DrawXAxisH();
            if(Vertical) DrawYAxis(); else DrawYAxisH();

            for (int i = 0; i < figure.Contents.Count; ++i)
                {
                    (MmdJsonArray content, string type) = figure.Contents[i];
                    if (type == "line")
                        if (Vertical) DrawLine(content); else DrawLineH(content);
                    else if (type == "bar")
                        if (Vertical) DrawBar(content); else DrawBarH(content);
            }

            DrawTitle();
        }

        void DrawTitle() 
        {
            Shape title = DropText(figure.Title, 0, 0);            
            SetShapeSheet(title, "Char.Size", $"{FontSize(title) * 2}");
            AdjustSize(title);
            MoveTo(title, $"{ChartWidth / 2} mm", $"{ChartHeight + (Vertical ? 10 : 25)} mm");
            SetTextColor(title, VColor.Create(titleColor));
        }

        void DrawXAxis()
        {
            Shape x = visioPage.Drop(GetMaster("x-axis"), 0, 0);
            SetShapeSheet(x, "BeginX", "0");
            SetShapeSheet(x, "BeginY", "0");
            SetShapeSheet(x, "EndX", $"{ChartWidth} mm");
            SetShapeSheet(x, "EndY", "0");       
            SetText(x, xShowTitle, figure.XAxis.Title);
            SetTextColor(x, "config.themeVariables.secondaryTextColor");

            double width = Width(x);
            this.Assert(width > 0, "invalid x-axis width");

            Shape tick = visioPage.Drop(GetMaster("x-tag"), 0, 0);
            double startSpace = Height(tick);
            tick.Delete();

            double xstep = XTicks.Count > 1 ? ((width-startSpace) / XTicks.Count) : width;
            VShapeBoundary boundary = new();
            for (int i=0; i<XTicks.Count; ++i)
            {
                tick = visioPage.Drop(GetMaster("x-tag"), 0, 0);
                SetText(tick, xShowLabel, XTicks[i]?.ToString() ?? string.Empty);
                if (i == 0)
                    MoveTo(tick, Height(tick) / 2 + startSpace, -Width(tick) / 2); // DataSheet: Angle=90 deg
                else
                    MoveTo(tick, boundary.PinX + xstep, boundary.PinY);                  

                boundary = new VShapeBoundary(tick);
                xBoundList.Add(boundary);
            }

            SetShapeSheet(x, "VerticalAlign", "0");
            SetShapeSheet(x, "TopMargin", $"{boundary.Height}");
        }

        void DrawYAxis()
        {
            Shape y = visioPage.Drop(GetMaster("y-axis"), 0, 0);
            SetShapeSheet(y, "BeginX", "0");
            SetShapeSheet(y, "BeginY", "0");
            SetShapeSheet(y, "EndX", "0");
            SetShapeSheet(y, "EndY", $"{ChartHeight} mm");
            SetText(y, yShowTitle, figure.YAxis.Title);
            SetTextColor(y, "config.themeVariables.secondaryTextColor");

            double height = ShapeSheetIU(y, "EndY");
            this.Assert(height > 0, "invalid y-axis height");

            double ystep = YTicks.Count > 1 ? (height / YTicks.Count) : height;
            VShapeBoundary boundary = new();
            for (int i = 0; i < YTicks.Count; ++i)
            {
                Shape tick = visioPage.Drop(GetMaster("y-tag"), 0, 0);
                SetText(tick, yShowLabel, YTicks[i]?.ToString() ?? string.Empty);
                if (i == 0)
                    MoveTo(tick, -Width(tick) / 2, Height(tick) / 2); 
                else
                    MoveTo(tick, boundary.PinX, boundary.PinY + ystep);

                boundary = new VShapeBoundary(tick);
                yBoundList.Add(boundary);
            }

            SetShapeSheet(y, "VerticalAlign", "0");
            SetShapeSheet(y, "TopMargin", $"{boundary.Height}");
        }

        void DrawLine(MmdJsonArray line)
        {
            PointF start = Point.Empty, end = Point.Empty;
            VRGBColor rgb = NextColor();
            for (int i=0; i<Math.Min(line.Count, xBoundList.Count); ++i)
            {
                string? yValue = line[i]?.ToString();
                double yPos = CalcYPos(yValue);
                if (i == 0) start = new((float) xBoundList[i].PinX, (float) yPos);
                else
                {
                    end = new((float) xBoundList[i].PinX, (float) yPos);
                    Shape s = visioPage.Drop(GetMaster("line"), 0, 0);
                    SetShapeSheet(s, "BeginX", $"{start.X}");
                    SetShapeSheet(s, "BeginY", $"{start.Y}");
                    SetShapeSheet(s, "EndX", $"{end.X}");
                    SetShapeSheet(s, "EndY", $"{end.Y}");
                    SetShapeSheet(s, "LineWeight", "=1.25 pt");
                    SetLineColor(s, rgb);

                    start = end;
                }
            }
        }

        void DrawBar(MmdJsonArray bar)
        {
            VRGBColor rgb = NextColor();
            for (int i = 0; i < Math.Min(bar.Count, xBoundList.Count); ++i)
            {
                string? yValue = bar[i]?.ToString();
                double yPos = CalcYPos(yValue);

                Shape s = visioPage.Drop(GetMaster("[]"), 0, 0);
                SetWidth(s, xBoundList.First().Height);
                SetHeight(s, Math.Abs(yPos));
                MoveTo(s, xBoundList[i].PinX, yPos / 2);
                s.SendToBack();

                SetFillForegnd(s, rgb);
                SetLineColor(s, rgb);
            }
        }

        void DrawXAxisH()
        {
            Shape x = visioPage.Drop(GetMaster("y-axis"), 0, 0);
            SetShapeSheet(x, "BeginX", "0");
            SetShapeSheet(x, "BeginY", "0");
            SetShapeSheet(x, "EndX", "0");
            SetShapeSheet(x, "EndY", $"{ChartHeight} mm");
            x.Text = figure.XAxis.Title;
            SetTextColor(x, "config.themeVariables.secondaryTextColor");

            double height = ShapeSheetIU(x, "EndY");
            this.Assert(height > 0, "invalid x-axis width");

            double xstep = XTicks.Count > 1 ? (height / XTicks.Count) : height;
            VShapeBoundary boundary = new();
            for (int i = 0; i < XTicks.Count; ++i)
            {
                Shape tick = visioPage.Drop(GetMaster("y-tag"), 0, 0);
                tick.Text = XTicks[i]?.ToString() ?? string.Empty;
                if (i == 0)
                    MoveTo(tick, -Width(tick) / 2, Height(tick) / 2); 
                else
                    MoveTo(tick, boundary.PinX, boundary.PinY + xstep);

                boundary = new VShapeBoundary(tick);
                xBoundList.Add(boundary);
            }

            SetShapeSheet(x, "VerticalAlign", "0");
            SetShapeSheet(x, "TopMargin", $"{boundary.Height}");
        }

        void DrawYAxisH()
        {
            Shape y = visioPage.Drop(GetMaster("y-axis-h"), 0, 0);
            SetShapeSheet(y, "BeginX", "0");
            SetShapeSheet(y, "BeginY", $"{ChartHeight + 6} mm");
            SetShapeSheet(y, "EndX", $"{ChartWidth} mm");
            SetShapeSheet(y, "EndY", $"{ChartHeight + 6} mm");
            y.Text = figure.YAxis.Title;
            SetTextColor(y, "config.themeVariables.secondaryTextColor");

            double width = ShapeSheetIU(y, "EndX");
            double height = ShapeSheetIU(y, "EndY");
            this.Assert(width > 0, "invalid y-axis height");

            double ystep = YTicks.Count > 1 ? (width / YTicks.Count) : width;
            VShapeBoundary boundary = new();
            for (int i = 0; i < YTicks.Count; ++i)
            {
                Shape tick = visioPage.Drop(GetMaster("x-tag"), 0, 0);
                tick.Text = YTicks[i]?.ToString() ?? string.Empty;
                if (i == 0)
                    MoveTo(tick, Height(tick) / 2, -Width(tick) / 2 + height); // DataSheet: Angle=90 deg
                else
                    MoveTo(tick, boundary.PinX + ystep, boundary.PinY);

                boundary = new VShapeBoundary(tick);
                yBoundList.Add(boundary);
            }

            SetShapeSheet(y, "VerticalAlign", "0");
            SetShapeSheet(y, "TopMargin", $"{boundary.Height}");
        }

        void DrawLineH(MmdJsonArray line)
        {
            PointF start = Point.Empty, end = Point.Empty;
            VRGBColor rgb = NextColor();
            for (int i = 0; i < Math.Min(line.Count, xBoundList.Count); ++i)
            {
                string? yValue = line[i]?.ToString();
                double yPos = CalcYPos(yValue);
                if (i == 0) start = new((float)yPos, (float)xBoundList[i].PinY);
                else
                {
                    end = new((float)yPos, (float)xBoundList[i].PinY);
                    Shape s = visioPage.Drop(GetMaster("line"), 0, 0);
                    SetShapeSheet(s, "BeginX", $"{start.X}");
                    SetShapeSheet(s, "BeginY", $"{start.Y}");
                    SetShapeSheet(s, "EndX", $"{end.X}");
                    SetShapeSheet(s, "EndY", $"{end.Y}");
                    SetShapeSheet(s, "LineWeight", "=1.25 pt");
                    SetLineColor(s, rgb);

                    start = end;
                }
            }
        }

        void DrawBarH(MmdJsonArray bar)
        {
            VRGBColor rgb = NextColor();
            for (int i = 0; i < Math.Min(bar.Count, xBoundList.Count); ++i)
            {
                string? yValue = bar[i]?.ToString();
                double yPos = CalcYPos(yValue);

                Shape s = visioPage.Drop(GetMaster("[]"), 0, 0);
                SetHeight(s, xBoundList.First().Height);
                SetWidth(s, Math.Abs(yPos));
                MoveTo(s, yPos / 2, xBoundList[i].PinY);
                s.SendToBack();

                SetFillForegnd(s, rgb);
                SetLineColor(s, rgb);
            }
        }

        double CalcYPos(string? yValue)
        {
            double y = ParseY(yValue);
            double start = ParseY(yBoundList.First().Shape?.Text.Trim());
            double end = ParseY(yBoundList.Last().Shape?.Text.Trim());

            double startPos = Vertical ? yBoundList.First().PinY : yBoundList.First().PinX;
            double endPos = Vertical ? yBoundList.Last().PinY : yBoundList.Last().PinX;

            double yPos = startPos + (endPos - startPos) * (y - start) / (end - start);
            return yPos;
        }

        double ParseY(string? yValue)
        {
            double y;
            this.Assert(double.TryParse(yValue, out y), $"invalid y-axis value '{yValue}'");

            return y;
        }

        VRGBColor NextColor()
        {
            this.Assert(colorList.Count > 0, "can't load 'config.xyChart.plotColorPalette'");
            return (VRGBColor) VRGBColor.Create(colorList[colorIndex++ % colorList.Count]);
        }

        void SetText(Shape shape, string showText, string text)
        {
            shape.Text = "true".Equals(showText.ToLower()) ? text : "";            
        }
    }
}
