using md2visio.struc.figure;
using md2visio.vsdx;

namespace md2visio.struc.xy
{
    internal class XyChart : Figure
    {
        public XyAxis XAxis { get; set; } = Empty.Get<XyAxis>();
        public XyAxis YAxis { get; set; } = Empty.Get<XyAxis>();
        public List<(MmdJsonArray, string type)> Contents { get; set; } = new();

        public bool IsVertical
        {
            get
            {   
                Config.GetString("config.xyChart.chartOrientation", out string ori);
                return !ori.ToLower().Equals("horizontal");
            }
        }

        public XyChart() {
            
        }

        public override void ToVisio(string path)
        {
            new VBuilderXy(this).Build(path);
        }
    }
}
