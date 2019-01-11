using System;
using System.Collections.Generic;
using System.Xml;
using UIPathValidator.UIPath;

namespace UIPathValidator
{
    public class WorkflowValidator : IValidator
    {
        protected Workflow Workflow { get; set; }

        protected IEnumerable<ValidationResult> Results { get; set; }

        protected XmlReader Reader { get; set; }

        public WorkflowValidator(Workflow workflow)
        {
            this.Workflow = workflow;
            Results = new List<ValidationResult>();
        }

        public void Validate()
        {
            
        }
    }
}