using System.IO;

namespace UIPathValidator.UIPath
{
    public class Workflow
    {
        public string FilePath { get; protected set; }

        public GraphColor Color { get; set; }

        public UseStatus UseStatus { get; set; }

        public Workflow(string path)
        {
            path = Path.GetFullPath(path);
            if (File.Exists(path))


            FilePath = path;
        }
    }
}