using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace UIPathValidator.UIPath
{
    public class Workflow
    {
        public Project Project { get; set; }

        public string FilePath { get; protected set; }

        public GraphColor Color { get; set; }

        public UseStatus UseStatus { get; set; }

        public List<Argument> Arguments { get; protected set; }

        public List<Variable> Variables { get; protected set; }

        public HashSet<Workflow> ConnectedWorkflow { get; protected set; }

        public bool HasDynamicallyInvokedWorkflows { get; set; }

        public bool Parsed { get; protected set; }

        protected XamlReader Reader { get; set; }

        public decimal DelayOnActivities { get; set; }

        public decimal DelayOnAttributes { get; set; }

        public decimal DelayTotal => DelayOnActivities + DelayOnAttributes;

        public string RelativePath
        {
            get
            {
                return PathHelper.MakeRelativePath(FilePath, Project.Folder);
            }
        }

        public Workflow(Project project, string path)
        {
            Project = project;
            
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                throw new FileNotFoundException("The specified workflow file was not found.");

            FilePath = path;

            Parsed = false;
            Color = GraphColor.White;
            UseStatus = UseStatus.NotMentioned;
            Arguments = new List<Argument>();
            Variables = new List<Variable>();
            ConnectedWorkflow = new HashSet<Workflow>();
        }

        public XamlReader GetXamlReader()
        {
            return Reader;
        }

        public void EnsureParse()
        {
            if (!Parsed)
                ParseFile();
        }

        public void ParseFile()
        {
            Reader = new XamlReader(FilePath);

            // Read Arguments
            if (Reader.Namespaces.HasNamespace("x"))
            {
                var members = Reader.Document.Root.Elements(XName.Get("Members", Reader.Namespaces.LookupNamespace("x")));
                ParseArguments(members, Reader.Namespaces);
            }

            // Read Variables
            var variables = Reader.Document.Root.Descendants(XName.Get("Variable", Reader.Namespaces.DefaultNamespace));
            ParseVariables(variables, Reader.Namespaces);

            Parsed = true;
        }

        protected void ParseArguments(IEnumerable<XElement> members, XmlNamespaceManager namespaces)
        {
            if (!namespaces.HasNamespace("x"))
                return;

            var arguments = members.Elements(XName.Get("Property", namespaces.LookupNamespace("x")));
            foreach (var arg in arguments)
            {
                var name = arg.Attribute("Name").Value;
                var type = arg.Attribute("Type").Value;
                var argument = Argument.CreateFromAttributes(name, type);
                Arguments.Add(argument);
            }
        }

        private void ParseVariables(IEnumerable<XElement> variables, XmlNamespaceManager namespaces)
        {
            if (!namespaces.HasNamespace("x"))
                return;
            
            foreach (var variab in variables)
            {
                var name = variab.Attribute("Name").Value;
                var type = variab.Attribute(XName.Get("TypeArguments", namespaces.LookupNamespace("x"))).Value;
                var context = variab.Parent.Parent.Attribute("DisplayName")?.Value;
                var newVar = new ActivityVariable(name, type, context);
                Variables.Add(newVar);
            }
        }
    }
}