using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    internal class IfValidationResult : ValidationResult
    {
        public string TaskName { get; set; }
        public Workflow Workflow { get; set; }

        public override string FormattedMessage
        {
            get
            {
                return string.Format("{0} - {1}: {2}", Workflow.RelativePath, TaskName, Message);
            }
        }

        public IfValidationResult(Workflow workflow) : this(workflow, "If") { }

        public IfValidationResult(Workflow workflow, string taskName) : this(workflow, taskName, ValidationResultType.Info) { }

        public IfValidationResult(Workflow workflow, string taskName, ValidationResultType type) : this(workflow, taskName, type, string.Empty) { }

        public IfValidationResult(Workflow workflow, string taskName, ValidationResultType type, string message) : base(message, type)
        {
            TaskName = taskName;
            Workflow = workflow;
        }
    }
}