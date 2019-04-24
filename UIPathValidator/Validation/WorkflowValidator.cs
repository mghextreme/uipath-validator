using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Referees;
using UIPathValidator.Validation.Result;

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

            AddResults(new ArgumentNameReferee().Validate(Workflow));
            AddResults(new VariableNameReferee().Validate(Workflow));
            GetAndValidateInvokes();
            AddResults(new EmptyIfReferee().Validate(Workflow));
            AddResults(new FlowchartReferee().Validate(Workflow));
            AddResults(new EmptySequenceReferee().Validate(Workflow));
            AddResults(new EmptyWhileReferee().Validate(Workflow));
            AddResults(new EmptyDoWhileReferee().Validate(Workflow));
            AddResults(new MinimalTryCatchReferee().Validate(Workflow));
            AddResults(new CommentOutReferee().Validate(Workflow));
            AddResults(new DelayReferee().Validate(Workflow));
        }

        private void GetAndValidateInvokes()
        {
            var reader = Workflow.GetXamlReader();

            if (!reader.Namespaces.HasNamespace("ui"))
                return;

            var invokes = reader.Document.Descendants(XName.Get("InvokeWorkflowFile", reader.Namespaces.LookupNamespace("ui")));
            var workflowFolder = Path.GetDirectoryName(Workflow.FilePath);

            foreach (var invoke in invokes)
            {
                if (invoke.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var name = invoke.Attribute("DisplayName")?.Value;
                var file = invoke.Attribute("WorkflowFileName")?.Value;

                if (file.StartsWith("[") && file.EndsWith("]"))
                {
                    this.Workflow.HasDynamicallyInvokedWorkflows = true;
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
                    fileFullPath = Path.Combine(this.Workflow.Project.Folder, file);
                    if (!File.Exists(fileFullPath))
                        fileFullPath = Path.Combine(workflowFolder, file);
                }

                fileRelativePath = PathHelper.MakeRelativePath(fileFullPath, this.Workflow.Project.Folder);
                invokedWorkflow = this.Workflow.Project.GetWorkflow(fileRelativePath);

                if (invokedWorkflow == null)
                {
                    AddResult(new InvokeValidationResult(this.Workflow, fileFullPath, name, ValidationResultType.Error, $"The workflow path was not found in the project folder."));
                    continue;
                }

                this.Workflow.ConnectedWorkflow.Add(invokedWorkflow);

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
                CheckInvokedArguments(invokedWorkflow, arguments, name);
            }
        }

        private void CheckInvokedArguments(Workflow workflow, Dictionary<string, Argument> arguments, string displayName)
        {
            workflow.EnsureParse();

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
    }
}