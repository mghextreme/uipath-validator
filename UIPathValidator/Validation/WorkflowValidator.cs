using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    public class WorkflowValidator : Validator
    {
        protected Workflow Workflow { get; set; }

        public WorkflowValidator(Workflow workflow) : base()
        {
            this.Workflow = workflow;
        }

        public override void Validate()
        {
            if (!Workflow.Parsed)
                Workflow.ParseFile();

            ValidateArguments();
            ValidateVariables();
            GetAndValidateInvokes();
        }

        protected void ValidateArguments()
        {
            foreach (var argument in Workflow.Arguments)
            {
                if (!argument.Name.StartsWith(argument.Direction.Prefix()))
                {
                    AddResult(new ArgumentValidationResult(argument.Name, Workflow, ValidationResultType.Warning, $"{argument.Direction.ToString()}Argument doesn't start with prefix '{argument.Direction.Prefix()}'."));
                }
                else
                {
                    var underscorePos = argument.Name.IndexOf('_');
                    var name = argument.Name.Substring(underscorePos + 1);
                    var ascName = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(name));

                    if (!name.Equals(ascName))
                    {
                        AddResult(new ArgumentValidationResult(argument.Name, Workflow, ValidationResultType.Warning, $"Argument contains invalid non-ASCII characters."));
                    }
                    
                    if (name[0] > 90)
                    {
                        AddResult(new ArgumentValidationResult(argument.Name, Workflow, ValidationResultType.Warning, $"Argument doesn't start with a capital letter."));
                    }
                }
            }
        }

        protected void ValidateVariables()
        {
            foreach (var variable in Workflow.Variables)
            {
                if (variable.Name[0] > 90)
                {
                    AddResult(new VariableValidationResult(variable, Workflow, ValidationResultType.Warning, $"Variable doesn't start with a capital letter."));
                }
            }
        }

        private void GetAndValidateInvokes()
        {
            var reader = Workflow.GetXamlReader();
            var invokes = reader.Document.Descendants(XName.Get("InvokeWorkflowFile", reader.Namespaces.LookupNamespace("ui")));

            foreach (var invoke in invokes)
            {
                var name = invoke.Parent.Parent.Attribute("DisplayName")?.Value;
                var file = invoke.Parent.Parent.Attribute("WorkflowFileName")?.Value;
                var fileRelativePath = PathHelper.MakeRelativePath(file, this.Workflow.Project.Folder);

                Workflow workflow = this.Workflow.Project.GetWorkflow(fileRelativePath);
                if (workflow == null)
                {
                    AddResult(new InvokeValidationResult(this.Workflow, file, name, ValidationResultType.Error, $"The workflow path was not found in the project folder."));
                    continue;
                }
            }
        }
    }
}