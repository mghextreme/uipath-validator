using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using UIPathValidator.UIPath;

namespace UIPathValidator
{
    public class WorkflowValidator : IValidator
    {
        const string xmlnsx = @"http://schemas.microsoft.com/winfx/2006/xaml";

        protected Workflow Workflow { get; set; }

        public List<ValidationResult> Results { get; protected set; }

        public WorkflowValidator(Workflow workflow)
        {
            this.Workflow = workflow;
            Results = new List<ValidationResult>();
        }

        public void Validate()
        {
            using (TextReader textReader = File.OpenText(Workflow.FilePath))
            {
                var doc = XDocument.Load(textReader);
                
                var members = doc.Root.Elements(XName.Get("Members", xmlnsx));
                ValidateArguments(members);
            }

        }

        private void ValidateArguments(IEnumerable<XElement> members)
        {
            var arguments = members.Elements(XName.Get("Property", xmlnsx));
            foreach (var arg in arguments)
            {
                var name = arg.Attribute("Name").Value;
                var type = arg.Attribute("Type").Value;
                var argType = type.Substring(0, type.IndexOf('('));

                switch (argType)
                {
                    case "InArgument":
                        ValidateArgument(ArgumentType.In, name);
                        break;
                    case "InOutArgument":
                        ValidateArgument(ArgumentType.InOut, name);
                        break;
                    case "OutArgument":
                        ValidateArgument(ArgumentType.Out, name);
                        break;
                }
            }

        }

        private void ValidateArgument(ArgumentType type, string name)
        {
            switch (type)
            {
                case ArgumentType.In:
                    if (!name.StartsWith("in_"))
                    {
                        Console.WriteLine($"Argument {name} in {Workflow.FilePath} doesn't start with 'in_'.");
                        return;
                    }
                    break;
                case ArgumentType.InOut:
                    if (!name.StartsWith("io_"))
                    {
                        Console.WriteLine($"Argument {name} in {Workflow.FilePath} doesn't start with 'io_'.");
                        return;
                    }
                    break;
                case ArgumentType.Out:
                    if (!name.StartsWith("out_"))
                    {
                        Console.WriteLine($"Argument {name} in {Workflow.FilePath} doesn't start with 'out_'.");
                        return;
                    }
                    break;
            }
        }
    }
}