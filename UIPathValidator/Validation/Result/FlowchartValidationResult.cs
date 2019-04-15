using UIPathValidator.UIPath;

namespace UIPathValidator.Validation.Result
{
    internal class FlowchartValidationResult : ValidationResult
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

        public FlowchartValidationResult(Workflow workflow) : this(workflow, "Flowchart") { }

        public FlowchartValidationResult(Workflow workflow, string taskName) : this(workflow, taskName, ValidationResultType.Info) { }

        public FlowchartValidationResult(Workflow workflow, string taskName, ValidationResultType type) : this(workflow, taskName, type, string.Empty) { }

        public FlowchartValidationResult(Workflow workflow, string taskName, ValidationResultType type, string message) : base(message, type)
        {
            TaskName = taskName;
            Workflow = workflow;
        }
    }
}