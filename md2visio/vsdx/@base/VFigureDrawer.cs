using md2visio.struc.figure;
using md2visio.vsdx.@tool;
using Microsoft.Office.Interop.Visio;

namespace md2visio.vsdx.@base
{
    internal abstract class VFigureDrawer<T> :
        VShapeDrawer where T : Figure
    {
        protected T figure = Empty.Get<T>();
        protected Config config;

        public VFigureDrawer(T figure, Application visioApp) : base(visioApp)
        {
            this.figure = figure;
            config = figure.Config;
        }

        public abstract void Draw();

        public void SetFillForegnd(Shape shape, string configPath)
        {
            if (config.GetString(configPath, out string sColor))
            {
                SetFillForegnd(shape, VColor.Create(sColor));
            }
        }

        public void SetFillForegnd(Shape shape, VColor color)
        {
            SetShapeSheet(shape, "FillPattern", "1");
            SetShapeSheet(shape, "FillForegnd", $"THEMEGUARD({color.RGB()})");
        }

        public void SetLineColor(Shape shape, string configPath)
        {
            if(config.GetString(configPath, out string color)) {
                SetShapeSheet(shape, "LineColor", $"THEMEGUARD({VColor.Create(color).RGB()})");
            }
        }

        public void SetTextColor(Shape shape, string configPath)
        {
            if (config.GetString(configPath, out string color)) {
                shape.CellsU["Char.Color"].FormulaU = $"THEMEGUARD({VColor.Create(color).RGB()})";
            }
        }

        public List<string> GetStringList(string prefix, int maxCount = 13)
        {
            List<string> list = figure.Config.GetStringList(prefix, maxCount);
            if(list.Count == 0) 
                list = figure.Config.GetStringListFromMultipleKeys(prefix); 

            return list;
        }

    }
}
