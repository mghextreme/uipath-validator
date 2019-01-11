using System;
using System.Collections.Generic;
using UIPathValidator.UIPath;

namespace UIPathValidator
{
    public class ProjectValidator : IValidator
    {
        protected Project Project { get; set; }

        public List<ValidationResult> Results { get; protected set; }

        public ProjectValidator(Project project) 
        {
            this.Project = project;
            Results = new List<ValidationResult>();
        }

        public void Validate()
        {
            Project.EnsureLoad();

            // First, validate files individually
            foreach (Workflow workflow in Project.Workflows.Values)
            {
                var validator = new WorkflowValidator(workflow);
                validator.Validate();
                Results.AddRange(validator.Results);
            }
        }
    }
}