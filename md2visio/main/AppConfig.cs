namespace md2visio.main
{
    internal class AppConfig
    {
        public static readonly AppConfig Instance = new AppConfig();

        public string InputFile { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public bool Visible { get; set; } = false; 
        public bool Quiet { get; set; } = false;
        public bool Debug { get; set; } = false;
        public bool Test {  get; set; } = false;

        AppConfig() { }

        public bool ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToUpper();
                if(arg=="/I" && i + 1 < args.Length)
                    InputFile = args[++i];
                else if(arg=="/O" && i + 1 < args.Length)
                    OutputPath = args[++i];
                else if(arg=="/Y") 
                    Quiet = true;
                else if(arg=="/V")
                    Visible = true;
                else if (arg == "/D")
                    Debug = true;
                else if (arg == "/T")
                    Test = true;
                else if(arg == "/?" 
                    || arg == "/H"   || arg == "/HELP"
                    || arg == "-H"   || arg == "-HELP"
                    || arg == "--H"  || arg == "--HELP")
                    ShowHelp();
            }

            bool success = InputFile != string.Empty && 
                OutputPath != string.Empty;
            if(!success) ShowHelp();

            return success;
        }

        void ShowHelp()
        {
            Console.WriteLine(
                "md2visio /I MD_FILE /O OUTPUT_PATH [/V] [/Y]\n\n" +
                "/I\tSpecify the path of the input file (.md)\n" +
                "/O\tSpecify the path/folder of the output file (.vsdx)\n" +
                "/V\tShow the Visio application while drawing (optional, default is invisible)\n" +
                "/Y\tQuietly overwrite the existing output file (optional, by default requires user confirmation\n" +
                "/?\tPrint this help");
        }
    }
}
