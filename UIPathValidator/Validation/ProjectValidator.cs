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

            // First, validate files individually
            foreach (Workflow workflow in Project.Workflows.Values)
            {
                var validator = new WorkflowValidator(workflow);
                validator.Validate();
                Results.AddRange(validator.GetResults());
            }
        }
    }
}