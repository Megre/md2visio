using md2visio.mermaid.cmn;
using System.Reflection;
using md2visio.vsdx.@base;

namespace md2visio.struc.figure
{
    internal class FigureBuilderFactory
    {
        string outputFile;
        string? dir = string.Empty, name = string.Empty;
        Dictionary<string, Type> builderDict = TypeMap.BuilderMap;
        SttIterator iter;
        int count = 1;

        public FigureBuilderFactory(SttIterator iter)
        {
            this.iter = iter;
            outputFile = iter.Context.InputFile;
        }

        public void Build(string outputFile)
        {
            this.outputFile = outputFile;
            InitOutputPath();
            BuildFigures();
        }
        public void BuildFigures()
        {
            while (iter.HasNext())
            {
                List<SynState> list = iter.Context.StateList;
                for (int pos = iter.Pos + 1; pos < list.Count; ++pos)
                {
                    string word = list[pos].Fragment;
                    if (SttFigureType.IsFigure(word)) 
                        BuildFigure(word);
                }
            }            
        }
        public void Quit()
        {
            VBuilder.VisioApp.Quit();
        }

        void BuildFigure(string figureType)
        {
            if (!builderDict.ContainsKey(figureType)) 
                throw new NotImplementedException($"'{figureType}' builder not implemented");

            Type type = builderDict[figureType];
            object? obj = Activator.CreateInstance(type, iter);
            MethodInfo? method = type.GetMethod("Build", BindingFlags.Public | BindingFlags.Instance, null,
                new Type[] { typeof(string) }, null);

            method?.Invoke(obj, new object[] { $"{dir}\\{name}{count++}.vsdx" });
        }

        void InitOutputPath()
        {
            if (outputFile.ToLower().EndsWith(".vsdx") || File.Exists(outputFile)) // file
            {
                name = Path.GetFileNameWithoutExtension(outputFile);
                dir = Path.GetDirectoryName(outputFile);
            }
            else // directory
            {
                if (!Directory.Exists(outputFile))
                    throw new FileNotFoundException($"output path doesn't exist: '{outputFile}'");
                name = Path.GetFileNameWithoutExtension(iter.Context.InputFile);
                dir = Path.GetFullPath(outputFile).TrimEnd(new char[] { '/', '\\' });
            }
        }
    }
}
