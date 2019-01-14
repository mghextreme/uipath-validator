using System;
using System.Collections.Generic;
using System.Linq;
using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    public class ProjectValidator : Validator
    {
        protected Project Project { get; set; }

        public ProjectValidator(Project project) : base()
        {
            this.Project = project;
        }

        public override void Validate()
        {
            Project.EnsureLoad();

            var workflows = Project.GetWorkflows();

            // First, validate files individually
            foreach (Workflow workflow in workflows)
            {
                var validator = new WorkflowValidator(workflow);
                validator.Validate();
                Results.AddRange(validator.GetResults());
            }

            // Validate InvokeWorkflow graph
            ResetWorkflowsColors();
            var stack = new Stack<Workflow>();

            if (Project.InitialWorkflow != null)
            {
                Project.InitialWorkflow.UseStatus = UseStatus.Used;
                PaintWorfklow(Project.InitialWorkflow, stack);
            }
            else
            {
                foreach (var workflow in Project.GetWorkflows())
                    PaintWorfklow(workflow, stack);
            }
            
            var notUsed = 
                from workflow in Project.GetWorkflows()
                    where workflow.UseStatus == UseStatus.NotMentioned
                select workflow;

            foreach (var workflow in notUsed)
            {
                var message = "The workflow is unreachable because it is never invoked. Should this file be removed?";
                Results.Add(new UnreachableWorkflowValidationResult(workflow, ValidationResultType.Warning, message));
            }
        }

        protected void ResetWorkflowsColors()
        {
            foreach (var workflow in Project.GetWorkflows())
            {
                workflow.Color = GraphColor.White;
                workflow.UseStatus = UseStatus.NotMentioned;
            }
        }

        protected void PaintWorfklow(Workflow workflow, Stack<Workflow> stack)
        {
            if (workflow.Color == GraphColor.Black)
                return;

            if (workflow.Color == GraphColor.Gray)
            {
                string cycleText = GetCycleText(stack, workflow);
                string message = string.Format("The workflow contains a recursive invoke cycle. Make sure it is not an infinite loop: {0}.", cycleText);
                var temp = stack.Pop();
                AddResult(new InvokeValidationResult(stack.Peek(), workflow.FilePath, string.Empty, ValidationResultType.Info, message));
                stack.Push(temp);
                return;
            }

            if (stack.Count > 0)
                workflow.UseStatus = UseStatus.Used;

            workflow.Color = GraphColor.Gray;
            stack.Push(workflow);

            foreach (var sub in workflow.ConnectedWorkflow)
                PaintWorfklow(sub, stack);

            stack.Pop();
            workflow.Color = GraphColor.Black;
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