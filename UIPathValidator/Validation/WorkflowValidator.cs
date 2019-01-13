using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            ValidateCommentedActivities();
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
                        AddResult(new ArgumentValidationResult(argument.Name, Workflow, ValidationResultType.Warning, $"Argument contains invalid non-ASCII characters."));
                    
                    if (name[0] > 90)
                        AddResult(new ArgumentValidationResult(argument.Name, Workflow, ValidationResultType.Warning, $"Argument doesn't start with a capital letter."));
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
            var workflowFolder = Path.GetDirectoryName(Workflow.FilePath);

            foreach (var invoke in invokes)
            {
                var name = invoke.Attribute("DisplayName")?.Value;
                var file = invoke.Attribute("WorkflowFileName")?.Value;
                if (!Path.IsPathRooted(file))
                    file = Path.Combine(workflowFolder, file);
                var fileRelativePath = PathHelper.MakeRelativePath(file, this.Workflow.Project.Folder);

                Workflow workflow = this.Workflow.Project.GetWorkflow(fileRelativePath);
                if (workflow == null)
                {
                    AddResult(new InvokeValidationResult(this.Workflow, file, name, ValidationResultType.Error, $"The workflow path was not found in the project folder."));
                    continue;
                }

                var argumentsParent = invoke.Elements(XName.Get("InvokeWorkflowFile.Arguments", reader.Namespaces.LookupNamespace("ui")));
                var argumentsElements = argumentsParent.Elements();

                var arguments = new Dictionary<string, Argument>();
                foreach (var argEl in argumentsElements)
                {
                    switch (argEl.Name.LocalName.ToLower())
                    {
                        case "inargument":
                        case "inoutargument":
                        case "outargument":
                            Argument arg = Argument.CreateFromArgumentNode(argEl, reader.Namespaces);
                            arguments.Add(arg.Name, arg);
                            break;
                    }
                }
                CheckInvokedArguments(workflow, arguments, name);
            }
        }

        private void CheckInvokedArguments(Workflow workflow, Dictionary<string, Argument> arguments, string displayName)
        {
            // Check if all invoked arguments have been called
            foreach (var arg in workflow.Arguments)
            {
                // Check if the argument is imported
                if (!arguments.ContainsKey(arg.Name))
                {
                    var message = string.Format("The workflow argument {0} is not being called by the invoke activity.", arg.Name);
                    AddResult(new InvokeValidationResult(this.Workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                }
                else
                {
                    Argument usedArg = arguments[arg.Name];

                    // Check the argument direction
                    if (arg.Direction != usedArg.Direction)
                    {
                        var message = string.Format("The argument {0} is of direction {1} but is used as {2}.", arg.Name, arg.Direction, usedArg.Direction);
                        AddResult(new InvokeValidationResult(this.Workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                        continue;
                    }

                    // Check the argument type
                    if (arg.Type != usedArg.Type)
                    {
                        var message = string.Format("The argument {0} is of type {1} but is used as {2}.", arg.Name, arg.Type, usedArg.Type);
                        AddResult(new InvokeValidationResult(this.Workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                        continue;
                    }
                }
            }

            // Check if there are spare arguments
            var spareArguments =
                from Argument arg in arguments.Values
                    where workflow.Arguments.Where(x => x.Name == arg.Name).Count() == 0
                select arg;
            
            if (spareArguments.Count() > 0)
            {
                foreach (Argument arg in spareArguments)
                {
                    var message = string.Format("The called argument {0} doesn't exists in the workflow.", arg.Name);
                    AddResult(new InvokeValidationResult(this.Workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                }
            }
        }

        private void ValidateCommentedActivities()
        {
            var reader = Workflow.GetXamlReader();
            var commentOutTags = reader.Document.Descendants(XName.Get("CommentOut", reader.Namespaces.LookupNamespace("ui")));

            foreach (var commentOut in commentOutTags)
            {
                var name = commentOut.Attribute("DisplayName")?.Value ?? "CommentOut";
                var message = "CommentOut activities should be removed from workflow.";
                AddResult(new CommentOutValidationResult(this.Workflow, name, ValidationResultType.Info, message));
            }
        }
    }
}