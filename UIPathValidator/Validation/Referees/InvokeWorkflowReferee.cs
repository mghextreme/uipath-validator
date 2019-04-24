using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class InvokeWorkflowReferee : IWorkflowReferee
    {
        public string Code => "invoke-workflow";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();

            if (!reader.Namespaces.HasNamespace("ui"))
                return results;

            var invokes = reader.Document.Descendants(XName.Get("InvokeWorkflowFile", reader.Namespaces.LookupNamespace("ui")));
            var workflowFolder = Path.GetDirectoryName(workflow.FilePath);

            foreach (var invoke in invokes)
            {
                if (invoke.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var name = invoke.Attribute("DisplayName")?.Value;
                var file = invoke.Attribute("WorkflowFileName")?.Value;

                if (file.StartsWith("[") && file.EndsWith("]"))
                {
                    workflow.HasDynamicallyInvokedWorkflows = true;
                    continue;
                }

                Workflow invokedWorkflow = null;
                string fileFullPath = string.Empty;
                string fileRelativePath = string.Empty;

                if (Path.IsPathRooted(file))
                {
                    fileFullPath = file;
                }
                else
                {
                    fileFullPath = Path.Combine(workflow.Project.Folder, file);
                    if (!File.Exists(fileFullPath))
                        fileFullPath = Path.Combine(workflowFolder, file);
                }

                fileRelativePath = PathHelper.MakeRelativePath(fileFullPath, workflow.Project.Folder);
                invokedWorkflow = workflow.Project.GetWorkflow(fileRelativePath);

                if (invokedWorkflow == null)
                {
                    results.Add(new InvokeValidationResult(workflow, fileFullPath, name, ValidationResultType.Error, $"The workflow path was not found in the project folder."));
                    continue;
                }

                workflow.ConnectedWorkflow.Add(invokedWorkflow);

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
                results.AddRange(CheckInvokedArguments(invokedWorkflow, arguments, name));
            }

            return results;
        }

        private ICollection<ValidationResult> CheckInvokedArguments(Workflow workflow, Dictionary<string, Argument> arguments, string displayName)
        {
            var results = new List<ValidationResult>();

            workflow.EnsureParse();

            // Check if all invoked arguments have been called
            foreach (var arg in workflow.Arguments)
            {
                // Check if the argument is imported
                if (!arguments.ContainsKey(arg.Name))
                {
                    var message = string.Format("The workflow argument {0} is not being called by the invoke activity.", arg.Name);
                    results.Add(new InvokeValidationResult(workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                }
                else
                {
                    Argument usedArg = arguments[arg.Name];

                    // Check the argument direction
                    if (arg.Direction != usedArg.Direction)
                    {
                        var message = string.Format("The argument {0} is of direction {1} but is used as {2}.", arg.Name, arg.Direction, usedArg.Direction);
                        results.Add(new InvokeValidationResult(workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                        continue;
                    }

                    // Check the argument type
                    if (arg.Type != usedArg.Type)
                    {
                        var message = string.Format("The argument {0} is of type {1} but is used as {2}.", arg.Name, arg.Type, usedArg.Type);
                        results.Add(new InvokeValidationResult(workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
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
                    results.Add(new InvokeValidationResult(workflow, workflow.FilePath, displayName, ValidationResultType.Error, message));
                }
            }

            return results;
        }
    }
}