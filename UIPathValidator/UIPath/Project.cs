using System;
using System.Collections.Generic;
using System.IO;
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

        public string ProjectFile { get; set; } = "project.json";

        protected string MainFile { get; set; }

        public Workflow InitialWorkflow { get; protected set; }

        public Dictionary<string, Workflow> Workflows { get; protected set; }

        private bool loaded;

        public Project(string directory)
        {
            Name = string.Empty;
            Version = string.Empty;
            StudioVersion = string.Empty;
            InitialWorkflow = null;
            Workflows = new Dictionary<string, Workflow>();
            loaded = false;

            directory = Path.GetFullPath(directory);
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException("The specified directory was not found.");
            
            Folder = directory;
        }

        public void Load()
        {
            if (loaded)
                throw new ApplicationException("This project has already been loaded.");

            string projectFile = Path.Combine(Folder, ProjectFile);
            if (!File.Exists(projectFile))
                throw new FileNotFoundException("The project file was not found.");
            
            ReadProjectInfo(projectFile);

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
                var wf = new Workflow(fullName);
                Workflows.Add(PathHelper.MakeRelativePath(fullName, Folder), wf);

                if (MainFile.Equals(fullName, StringComparison.InvariantCultureIgnoreCase))
                    InitialWorkflow = wf;
            }
        }
    }
}