using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class FlowchartReferee : IWorkflowReferee
    {
        public string Code => "flowchart";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var flowchartTags = reader.Document.Descendants(XName.Get("Flowchart", reader.Namespaces.DefaultNamespace));

            foreach (var flowchartTag in flowchartTags)
            {
                if (flowchartTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var startNode = flowchartTag.Element(XName.Get("Flowchart.StartNode", reader.Namespaces.DefaultNamespace));
                var name = flowchartTag.Attribute("DisplayName")?.Value ?? "Flowchart";

                if (startNode == null)
                {
                    var message = "Flowchart activity doesn't have a Start Node.";
                    results.Add(new FlowchartValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
                else
                {
                    var orphanNodes = flowchartTag.Elements(XName.Get("FlowStep", reader.Namespaces.DefaultNamespace));

                    if (orphanNodes.Count() > 0)
                    {
                        var message = string.Format("Flowchart contains {0} node{1} that will never be reached. Either use {2} or delete {2}.",
                            orphanNodes.Count(),
                            orphanNodes.Count() > 1 ? "s" : string.Empty,
                            orphanNodes.Count() > 1 ? "them" : "it");
                        results.Add(new FlowchartValidationResult(workflow, name, ValidationResultType.Warning, message));
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
                        results.Add(new FlowchartValidationResult(workflow, name, ValidationResultType.Info, message));
                    }
                }
            }

            return results;
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
    }
}