using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UIPathValidator.UIPath
{
    public class Project
    {
        public string Name { get; protected set; }
        
        public string Version { get; protected set; }

        public string StudioVersion { get; protected set; }

        public string Folder { get; protected set; }

        public string ProjectFile { get; protected set; } = "project.json";

        protected string MainFile { get; set; }

        public Workflow InitialWorkflow { get; protected set; }

        protected Dictionary<string, Workflow> Workflows { get; set; }

        private bool loaded;

        private bool hasProjectFile;

        public Project(string path)
        {
            Name = string.Empty;
            Version = string.Empty;
            StudioVersion = string.Empty;
            InitialWorkflow = null;
            Workflows = new Dictionary<string, Workflow>();
            loaded = false;

            path = Path.GetFullPath(path);
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                Folder = Path.GetFullPath(path);
            }
            else
            {
                ProjectFile = Path.GetFileName(path);
                Folder = Path.GetDirectoryName(path);
            }

            if (!Folder.EndsWith(Path.DirectorySeparatorChar.ToString()) || !Folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                Folder += Path.DirectorySeparatorChar;

            if (!Directory.Exists(Folder))
                throw new DirectoryNotFoundException("The specified directory was not found.");
        }

        public void Load()
        {
            if (loaded)
                throw new ApplicationException("This project has already been loaded.");

            string projectFile = Path.Combine(Folder, ProjectFile);
            if (!File.Exists(projectFile))
            {
                // Legacy versions
                hasProjectFile = false;
                Name = new DirectoryInfo(Folder).Name;
                Version = "Legacy";
                StudioVersion = "Legacy";
                MainFile = string.Empty;
                InitialWorkflow = null;
            }
            else
            {
                hasProjectFile = true;
                ReadProjectInfo(projectFile);
            }

            ReadProjectWorkflows(Folder);

            loaded = true;
        }

        public void EnsureLoad()
        {
            if (!loaded)
                Load();
        }

        private void ReadProjectInfo(string projectFile)
        {
            using (TextReader textReader = File.OpenText(projectFile))
            {
                using (JsonReader jsonReader = new JsonTextReader(textReader))
                {
                    JObject config = JObject.Load(jsonReader);

                    if (config.ContainsKey("name"))
                        Name = config["name"].Value<string>();
                    
                    if (config.ContainsKey("projectVersion"))
                        Version = config["projectVersion"].Value<string>();
                    
                    if (config.ContainsKey("studioVersion"))
                        StudioVersion = config["studioVersion"].Value<string>();
                    
                    if (config.ContainsKey("main"))
                    {
                        var main = config["main"].Value<string>();
                        MainFile = Path.Combine(Folder, main);
                    }
                }
            }
        }

        private void ReadProjectWorkflows(string directory)
        {
            var dirInfo = new DirectoryInfo(directory);
            var files = dirInfo.GetFiles("*.xaml", SearchOption.AllDirectories);
            
            foreach (var file in files)
            {
                string fullName = file.FullName;
                var wf = new Workflow(this, fullName);
                Workflows.Add(PathHelper.MakeRelativePath(fullName, Folder), wf);

                if (MainFile.Equals(fullName, StringComparison.InvariantCultureIgnoreCase))
                    InitialWorkflow = wf;
            }
        }

        public IEnumerable<Workflow> GetWorkflows()
        {
            if (Workflows.Count > 0)
                return Workflows.Values.ToArray();
            
            return new List<Workflow>();
        }

        public Workflow GetWorkflow(string workflowPath)
        {
            if (Workflows.ContainsKey(workflowPath))
                return Workflows[workflowPath];
            
            return null;
        }
    }
}