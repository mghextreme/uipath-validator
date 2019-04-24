using System.Collections.Generic;
using System.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class WorkflowGraphReferee : IProjectReferee
    {
        public string Code => "workflow-graph";

        public ICollection<ValidationResult> Validate(Project project)
        {
            var results = new List<ValidationResult>();

            project.ResetWorkflowsColors();
            var stack = new Stack<Workflow>();

            if (project.InitialWorkflow != null)
            {
                project.InitialWorkflow.UseStatus = UseStatus.Used;
                PaintWorfklow(project.InitialWorkflow, stack);
            }
            else
            {
                foreach (var workflow in project.GetWorkflows())
                    results.AddRange(PaintWorfklow(workflow, stack));
            }

            var notUsed = 
                from workflow in project.GetWorkflows()
                    where workflow.UseStatus == UseStatus.NotMentioned
                select workflow;

            foreach (var workflow in notUsed)
            {
                string message = project.HasDynamicallyInvokedWorkflows ?
                    "This workflow is never directly invoked. If it is unreachable consider removing it." :
                    "The workflow is unreachable because it is never invoked. Should this file be removed?";
                ValidationResultType type = project.HasDynamicallyInvokedWorkflows ?
                    ValidationResultType.Info :
                    ValidationResultType.Warning;

                results.Add(new UnreachableWorkflowValidationResult(workflow, type, message));
            }

            return results;
        }

        protected ICollection<ValidationResult> PaintWorfklow(Workflow workflow, Stack<Workflow> stack)
        {
            var results = new List<ValidationResult>();

            if (workflow.Color == GraphColor.Black)
                return results;

            if (workflow.Color == GraphColor.Gray)
            {
                string cycleText = GetCycleText(stack, workflow);
                string message = string.Format("The workflow contains a recursive invoke cycle. Make sure it is not an infinite loop: {0}.", cycleText);
                var temp = stack.Pop();
                results.Add(new InvokeValidationResult(stack.Peek(), workflow.FilePath, string.Empty, ValidationResultType.Info, message));
                stack.Push(temp);
                return results;
            }

            if (stack.Count > 0)
                workflow.UseStatus = UseStatus.Used;

            workflow.Color = GraphColor.Gray;
            stack.Push(workflow);

            foreach (var sub in workflow.ConnectedWorkflow)
                results.AddRange(PaintWorfklow(sub, stack));

            stack.Pop();
            workflow.Color = GraphColor.Black;

            return results;
        }

        private string GetCycleText(Stack<Workflow> stack, Workflow leaf)
        {
            var enumerator = stack.GetEnumerator();
            string cycleText = leaf.RelativePath;
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                cycleText = item.RelativePath + " -> " + cycleText;

                if (item == leaf)
                    break;
            }
            return cycleText;
        }
    }
}