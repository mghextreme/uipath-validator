using UIPathValidator.UIPath;

namespace UIPathValidator.Validation.Result
{
    public class CommentOutValidationResult : ValidationResult
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

        public CommentOutValidationResult(Workflow workflow) : this(workflow, "CommentOut") { }

        public CommentOutValidationResult(Workflow workflow, string taskName) : this(workflow, taskName, ValidationResultType.Info) { }

        public CommentOutValidationResult(Workflow workflow, string taskName, ValidationResultType type) : this(workflow, taskName, type, string.Empty) { }

        public CommentOutValidationResult(Workflow workflow, string taskName, ValidationResultType type, string message) : base(message, type)
        {
            TaskName = taskName;
            Workflow = workflow;
        }
    }
}