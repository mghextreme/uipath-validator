using UIPathValidator.UIPath;

namespace UIPathValidator.Validation.Result
{
    public class DelayValidationResult : ValidationResult
    {
        public Workflow Workflow { get; set; }

        public override string FormattedMessage
        {
            get
            {
                return string.Format("{0}: {1}", Workflow.RelativePath, Message);
            }
        }

        public DelayValidationResult(Workflow workflow) : this(workflow, ValidationResultType.Info) { }

        public DelayValidationResult(Workflow workflow, ValidationResultType type) : this(workflow, type, string.Empty) { }

        public DelayValidationResult(Workflow workflow, ValidationResultType type, string message) : base(message, type)
        {
            Workflow = workflow;
        }
    }
}