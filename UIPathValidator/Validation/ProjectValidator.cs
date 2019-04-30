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

            // First, validate files individually
            var workflows = Project.GetWorkflows();
            var workflowReferees = GetWorkflowReferees();
            foreach (Workflow workflow in workflows)
            {
                workflow.EnsureParse();
                foreach (IWorkflowReferee referee in workflowReferees)
                {
                    AddResults(referee.Validate(workflow));
                }
            }

            // Then validate project as a whole
            var projectReferees = GetProjectReferees();
            foreach (IProjectReferee referee in projectReferees)
            {
                AddResults(referee.Validate(Project));
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
            referees.Add(new EmptySwitchReferee());
            referees.Add(new MinimalTryCatchReferee());
            referees.Add(new StateMachineReferee());
            referees.Add(new CommentOutReferee());
            referees.Add(new DelayReferee());
            return referees;
        }

        private ICollection<IProjectReferee> GetProjectReferees()
        {
            var referees = new List<IProjectReferee>();
            referees.Add(new WorkflowGraphReferee());
            return referees;
        }
    }
}