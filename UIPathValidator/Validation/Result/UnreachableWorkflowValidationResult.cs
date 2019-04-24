using UIPathValidator.UIPath;

namespace UIPathValidator.Validation.Result
{
    public class UnreachableWorkflowValidationResult : ValidationResult, IWorkflowValidationResult
    {
        public Workflow Workflow { get; set; }

        public override string FormattedMessage
        {
            get
            {
                return string.Format("{0} - {1}", Workflow.RelativePath, Message);
            }
        }

        public UnreachableWorkflowValidationResult(Workflow workflow) : this(workflow, ValidationResultType.Warning) { }

        public UnreachableWorkflowValidationResult(Workflow workflow, ValidationResultType type) : this(workflow, type, string.Empty) { }

        public UnreachableWorkflowValidationResult(Workflow workflow, ValidationResultType type, string message) : base(message, type)
        {
            Workflow = workflow;
        }
    }
}