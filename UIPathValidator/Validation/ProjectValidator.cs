using System;
using System.Collections.Generic;
using System.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Referees;
using UIPathValidator.Validation.Result;

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
            var workflowReferees = GetWorkflowReferees();

            // First, validate files individually
            foreach (Workflow workflow in workflows)
            {
                workflow.EnsureParse();
                foreach (IWorkflowReferee referee in workflowReferees)
                {
                    Results.AddRange(referee.Validate(workflow));
                }
            }

            // Validate InvokeWorkflow graph
            Project.ResetWorkflowsColors();
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
                string message = Project.HasDynamicallyInvokedWorkflows ?
                    "This workflow is never directly invoked. If it is unreachable consider removing it." :
                    "The workflow is unreachable because it is never invoked. Should this file be removed?";
                ValidationResultType type = Project.HasDynamicallyInvokedWorkflows ?
                    ValidationResultType.Info :
                    ValidationResultType.Warning;

                Results.Add(new UnreachableWorkflowValidationResult(workflow, type, message));
            }
        }

        private ICollection<IWorkflowReferee> GetWorkflowReferees()
        {
            var referees = new List<IWorkflowReferee>();
            referees.Add(new ArgumentNameReferee());
            referees.Add(new VariableNameReferee());
            referees.Add(new InvokeWorkflowReferee());
            referees.Add(new EmptyIfReferee());
            referees.Add(new FlowchartReferee());
            referees.Add(new EmptySequenceReferee());
            referees.Add(new EmptyWhileReferee());
            referees.Add(new EmptyDoWhileReferee());
            referees.Add(new MinimalTryCatchReferee());
            referees.Add(new CommentOutReferee());
            referees.Add(new DelayReferee());
            return referees;
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