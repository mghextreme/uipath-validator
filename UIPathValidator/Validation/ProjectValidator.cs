using System;
using System.Collections.Generic;
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
            PaintWorfklow(Project.InitialWorkflow, stack);

            foreach (var workflow in Project.GetWorkflows())
            {
                // If it's White, it's isolated from the graph
                if (workflow.Color == GraphColor.White)
                {
                    var message = "The workflow is unreachable because it is never invoked. Should this file be removed?";
                    Results.Add(new UnreachableWorkflowValidationResult(workflow, ValidationResultType.Warning, message));
                }
            }
        }

        protected void ResetWorkflowsColors()
        {
            foreach (var workflow in Project.GetWorkflows())
                workflow.Color = GraphColor.White;
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
                AddResult(new InvokeValidationResult(stack.Peek(), workflow.FilePath, string.Empty, ValidationResultType.Warning, message));
                stack.Push(temp);
                return;
            }

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