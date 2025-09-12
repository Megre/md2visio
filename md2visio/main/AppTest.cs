using md2visio.mermaid.cmn;
using md2visio.struc.figure;
using md2visio.vsdx.@base;

namespace md2visio.main
{
    internal class AppTest
    {
        string testDir = @"test";
        string[] files = ["graph", "journey", "packet", "pie"];
        string outputPath = @$"{Environment.GetEnvironmentVariable("HOMEPATH")}\Desktop\TestOutput";

        public void Test()
        {
            if(!Path.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            AppConfig config = AppConfig.Instance;
            foreach (string file in files)
            {
                SynContext synContext = new(@$"{testDir}\{file}.md");
                SttMermaidStart.Run(synContext);
                Console.Write(synContext.ToString());

                new FigureBuilderFactory(synContext.NewSttIterator()).Build(outputPath);
            }
            VBuilder.VisioApp.Quit();
        }
    }
}
