namespace md2visio.vsdx._tool
{
    internal class FileTool
    {
        public static string ExtendPath(string path)
        {
            string? fullpath = Path.Combine(ExeDir()??"", path);
            if (Path.Exists(fullpath))
                return fullpath;

            fullpath = Path.Combine(WorkDir()??"", path);
            if (Path.Exists(fullpath))
                return fullpath;

            fullpath = FindFileInPath(path);
            if(fullpath != null)
                return fullpath;

            if (!Path.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");
            return path;
        }

        public static string? ExeDir()
        {
            return Path.GetDirectoryName(Environment.ProcessPath);
        }

        public static string WorkDir()
        {
            return Environment.CurrentDirectory;
        }

        public static string? FindFileInPath(string fileName)
        {
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
                return null;

            char separator = Path.PathSeparator; // Windows上是';'，Unix上是':'
            string[] pathDirectories = pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string directory in pathDirectories)
            {
                string cleanDirectory = directory.Trim();
                if (string.IsNullOrEmpty(cleanDirectory) || !Directory.Exists(cleanDirectory))
                    continue;

                string fullPath = Path.Combine(cleanDirectory, fileName);
                if (Path.Exists(fullPath))
                {
                    return fullPath; 
                }
            }

            return null;
        }
    }
}
