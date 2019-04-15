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
            ValidateIfActivities();
            ValidateFlowchartActivities();
            AddResults(new EmptySequenceReferee().Validate(Workflow));
            ValidateWhileActivities();
            ValidateDoWhileActivities();
            ValidateTryCatchActivities();
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
                if (IsInsideCommentOut(invoke, reader.Namespaces))
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

        private void ValidateIfActivities()
        {
            var reader = Workflow.GetXamlReader();
            var ifTags = reader.Document.Descendants(XName.Get("If", reader.Namespaces.DefaultNamespace));

            foreach (var ifTag in ifTags)
            {
                if (IsInsideCommentOut(ifTag, reader.Namespaces))
                    continue;

                var ifThenTag = ifTag.Elements(XName.Get("If.Then", reader.Namespaces.DefaultNamespace));
                var ifElseTag = ifTag.Elements(XName.Get("If.Else", reader.Namespaces.DefaultNamespace));

                if (ifThenTag.Count() == 0 &&
                    ifElseTag.Count() == 0)
                {
                    var name = ifTag.Attribute("DisplayName")?.Value ?? "If";
                    var message = "If activity has no activities inside.";
                    AddResult(new EmptyScopeValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }
            }
        }

        private void ValidateFlowchartActivities()
        {
            var reader = Workflow.GetXamlReader();
            var flowchartTags = reader.Document.Descendants(XName.Get("Flowchart", reader.Namespaces.DefaultNamespace));

            foreach (var flowchartTag in flowchartTags)
            {
                if (IsInsideCommentOut(flowchartTag, reader.Namespaces))
                    continue;

                var startNode = flowchartTag.Element(XName.Get("Flowchart.StartNode", reader.Namespaces.DefaultNamespace));
                var name = flowchartTag.Attribute("DisplayName")?.Value ?? "Flowchart";

                if (startNode == null)
                {
                    var message = "Flowchart activity doens't have a Start Node.";
                    AddResult(new FlowchartValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }
                else
                {
                    var orphanNodes = flowchartTag.Elements(XName.Get("FlowStep", reader.Namespaces.DefaultNamespace));

                    if (orphanNodes.Count() > 0)
                    {
                        var message = string.Format("Flowchart contains {0} node{1} that will never be reached. Either use {2} ot delete {2}.",
                            orphanNodes.Count(),
                            orphanNodes.Count() > 1 ? "s" : string.Empty,
                            orphanNodes.Count() > 1 ? "them" : "it");
                        AddResult(new FlowchartValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                    }

                    var decisionTags = startNode.Descendants(XName.Get("FlowDecision", reader.Namespaces.DefaultNamespace));
                    var switchTags = startNode.Descendants(XName.Get("FlowSwitch", reader.Namespaces.DefaultNamespace));
                    bool hasAnyDecisionTag = false;

                    if (decisionTags.Count() > 0)
                    {
                        foreach (var tag in decisionTags)
                        {
                            if (IsDirectlyInFlowchart(tag, startNode, reader.Namespaces))
                            {
                                hasAnyDecisionTag = true;
                                break;
                            }
                        }
                    }

                    if (!hasAnyDecisionTag && switchTags.Count() > 0)
                    {
                        foreach (var tag in switchTags)
                        {
                            if (IsDirectlyInFlowchart(tag, startNode, reader.Namespaces))
                            {
                                hasAnyDecisionTag = true;
                                break;
                            }
                        }
                    }

                    if (!hasAnyDecisionTag)
                    {
                        var message = "Flowchart activity doens't have any decisions. Shouldn't this be a sequence?";
                        AddResult(new FlowchartValidationResult(this.Workflow, name, ValidationResultType.Info, message));
                    }
                }
            }
        }

        private void ValidateWhileActivities()
        {
            var reader = Workflow.GetXamlReader();
            var whileTags = reader.Document.Descendants(XName.Get("While", reader.Namespaces.DefaultNamespace));

            foreach (var whileTag in whileTags)
            {
                if (IsInsideCommentOut(whileTag, reader.Namespaces))
                    continue;

                var insideTags = whileTag.Elements();

                if (insideTags.Count() == 0)
                {
                    var name = whileTag.Attribute("DisplayName")?.Value ?? "While";
                    var message = "While activity has no activities inside.";
                    AddResult(new EmptyScopeValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }
            }
        }

        private void ValidateDoWhileActivities()
        {
            var reader = Workflow.GetXamlReader();
            var doWhileTags = reader.Document.Descendants(XName.Get("DoWhile", reader.Namespaces.DefaultNamespace));

            foreach (var doWhileTag in doWhileTags)
            {
                if (IsInsideCommentOut(doWhileTag, reader.Namespaces))
                    continue;

                var insideTags = doWhileTag.Elements();

                if (insideTags.Count() == 0)
                {
                    var name = doWhileTag.Attribute("DisplayName")?.Value ?? "Do While";
                    var message = "Do While activity has no activities inside.";
                    AddResult(new EmptyScopeValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }
            }
        }

        private void ValidateTryCatchActivities()
        {
            var reader = Workflow.GetXamlReader();
            var tryCatchTags = reader.Document.Descendants(XName.Get("TryCatch", reader.Namespaces.DefaultNamespace));

            foreach (var tryCatchTag in tryCatchTags)
            {
                if (IsInsideCommentOut(tryCatchTag, reader.Namespaces))
                    continue;

                var name = tryCatchTag.Attribute("DisplayName")?.Value ?? "Do While";
                
                var tcTry = tryCatchTag.Element(XName.Get("TryCatch.Try", reader.Namespaces.DefaultNamespace));
                var tcCatches = tryCatchTag.Element(XName.Get("TryCatch.Catches", reader.Namespaces.DefaultNamespace));
                var tcFinally = tryCatchTag.Element(XName.Get("TryCatch.Finally", reader.Namespaces.DefaultNamespace));

                if (tcTry == null || tcTry.Elements().Count() == 0)
                {
                    var message = "Try Catch activity has no activities inside.";
                    AddResult(new EmptyScopeValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }

                if ((tcCatches == null || tcCatches.Elements().Count() == 0) &&
                    (tcFinally == null || tcFinally.Elements().Count() == 0))
                {
                    var message = "Try Catch activity has no catches and/or finally.";
                    AddResult(new EmptyScopeValidationResult(this.Workflow, name, ValidationResultType.Warning, message));
                }
            }
        }
        private bool IsDirectlyInFlowchart(XElement node, XElement flowchart, XmlNamespaceManager namespaces)
        {
            var startNodes = node.Ancestors(XName.Get("Flowchart.StartNode", namespaces.DefaultNamespace));
            
            if (startNodes.Count() == 0)
                return false;
            
            if (!namespaces.HasNamespace("sap2010"))
                return false;
            
            string firstParentNodeRefId = startNodes.First().Parent.Attribute(XName.Get("WorkflowViewState.IdRef", namespaces.LookupNamespace("sap2010")))?.Value;
            string flowchartRefId = flowchart.Parent.Attribute(XName.Get("WorkflowViewState.IdRef", namespaces.LookupNamespace("sap2010")))?.Value;
            if (flowchartRefId.Equals(firstParentNodeRefId))
                return true;
            
            return false;
        }

        private bool IsInsideCommentOut(XElement node, XmlNamespaceManager namespaces)
        {
            if (!namespaces.HasNamespace("ui"))
                return false;

            var ancestorComment = node.Ancestors(XName.Get("CommentOut", namespaces.LookupNamespace("ui")));
            return ancestorComment.Any();
        }
    }
}