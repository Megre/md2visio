using md2visio.struc.figure;
using md2visio.struc.xy;
using md2visio.vsdx.@base;
using System.Drawing;

namespace md2visio.vsdx
{
    internal class VBuilderXy: VFigureBuilder<XyChart>
    {
        double width, height; // mm
        bool vertical = true;
        VDrawerXy drawer;

        public VBuilderXy(XyChart figure): base(figure)
        {
            drawer = new VDrawerXy(figure, VisioApp);
        }

        protected override void ExecuteBuild()
        {           
            // config
            figure.Config.GetDouble("config.xyChart.width", out width);
            figure.Config.GetDouble("config.xyChart.height", out height);
            
            width = VShapeDrawer.Pix2MM() * width;
            height = VShapeDrawer.Pix2MM() * height;
            this.Assert(width > 0 && height > 0, "invalid xyChart width/height");

            
            // draw
            drawer.ChartWidth = width;
            drawer.ChartHeight = height;
            InitXTicks(drawer.XTicks);
            InitYTicks(drawer.YTicks);
            drawer.Draw();
        }

        void InitXTicks(List<string> ticks)
        {
            XyAxis x = figure.XAxis;
            if (x.Ticks.Count > 0)
            {
                foreach (string tick in x.Ticks) 
                    ticks.Add(tick);
            }
            else
            {
                int tickNum = ContentCount();
                this.Assert(tickNum > 0, "invalid xyChart line/bar");

                float start, end;
                if(x.Range != SizeF.Empty) (start, end) = (x.Range.Width, x.Range.Height);
                else (start, end) = (1, tickNum);

                float length = end - start;
                float step = (tickNum == 1 ? length : (length / (tickNum - 1)));
                for (int i = 1; i <= tickNum; ++i) 
                    ticks.Add(string.Format(step >= 1 ? "{0:F0}" : "{0:F1}", start + (i-1)*step));
            }
        }

        void InitYTicks(List<string> ticks)
        {
            XyAxis y = figure.YAxis;

            // range
            double start = 0, end = 0;
            if(y.Range != SizeF.Empty) 
                (start, end) = (y.Range.Width, y.Range.Height);
            else
                (start, end) = ContentRange();

            double step = GenerateNiceStep(start, end, ContentCount());
            double adjStart = AdjustedStart(start, step);
            int i;
            double tag;
            for (i = 1, tag = adjStart; tag <= end; ++i, tag = adjStart + (i - 1) * step)
                if(tag >= start) ticks.Add(string.Format(step >= 1 ? "{0:F0}" : "{0:F1}", tag));
        }

        public double GenerateNiceStep(double start, double end, int count)
        {
            if (count <= 0) throw new ArgumentException("Count must be positive.");
            if (Math.Abs(start - end) < 1e-6) return 1;

            double range = Math.Abs(end - start);
            double idealStep = range / count;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(idealStep)));
            double[] candidates = { 1 * magnitude, 2 * magnitude, 5 * magnitude };

            // 选择最接近 idealStep 的候选
            double bestStep = candidates[0];
            double minDiff = Math.Abs(candidates[0] - idealStep);

            foreach (double candidate in candidates)
            {
                double diff = Math.Abs(candidate - idealStep);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestStep = candidate;
                }
            }

            return bestStep;
        }

        public double AdjustedStart(double start, double step)
        {
            if (step <= 0) throw new ArgumentException("Step must be positive.");

            // 计算调整后的起始值：找到小于等于start的最大步长倍数
            double adjustedStart = Math.Floor(start / step) * step;

            return adjustedStart;
        }

        (float start, float end) ContentRange()
        {
            float minStart = 0, maxEnd = 0;

            foreach((MmdJsonArray content, string type) in figure.Contents)
            {
                float start = 0, end = 0;
                foreach (object value in content)
                {
                    if (value == content.First())
                    {
                        float.TryParse(value.ToString(), out start);
                        end = start;
                    }
                    else
                    {
                        float.TryParse(value.ToString(), out float f);
                        start = Math.Min(start, f);
                        end = Math.Max(end, f);
                    }
                }
                minStart = Math.Min(minStart, start);
                maxEnd = Math.Max(maxEnd, end);
            }
            
            return (minStart, maxEnd);
        }

        int ContentCount()
        {
            int count = 0;
            foreach ((MmdJsonArray content, string type) in figure.Contents)
            {
                count = Math.Max(count, content.Count);
            }
            return count;
        }

    }
}
