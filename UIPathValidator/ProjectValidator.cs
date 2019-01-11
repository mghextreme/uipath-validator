using System;
using System.Collections.Generic;
using UIPathValidator.UIPath;

namespace UIPathValidator
{
    public class ProjectValidator : IValidator
    {
        protected Project Project { get; set; }

        protected IEnumerable<ValidationResult> Results { get; set; }

        public ProjectValidator(Project project) 
        {
            this.Project = project;
            Results = new List<ValidationResult>();
        }

        public void Validate()
        {
            Project.EnsureLoad();
        }
    }
}