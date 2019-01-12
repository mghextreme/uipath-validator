using System.IO;
using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    internal class InvokeValidationResult : ValidationResult
    {
        public Workflow Workflow { get; set; }
        public string File { get; set; }
        public string TaskName { get; set; }

        public override string FormattedMessage
        {
            get
            {
                var where = PathHelper.MakeRelativePath(File, Workflow.Project.Folder);
                if (!string.IsNullOrWhiteSpace(TaskName))
                    where += $" ({TaskName})";
                return string.Format("{0} - {1}: {2}", Workflow.RelativePath, where, Message);
            }
        }

        public InvokeValidationResult(Workflow workflow, string file) : base(ValidationResultType.Info)
        {
            this.Workflow = workflow;
            this.File = file;
        }

        public InvokeValidationResult(Workflow workflow, string file, ValidationResultType type) : base(type)
        {
            this.Workflow = workflow;
            this.File = file;
        }

        public InvokeValidationResult(Workflow workflow, string file, string taskName, ValidationResultType type) : base(type)
        {
            this.Workflow = workflow;
            this.File = file;
            this.TaskName = taskName;
        }

        public InvokeValidationResult(Workflow workflow, string file, string taskName, ValidationResultType type, string message) : base(message, type)
        {
            this.Workflow = workflow;
            this.File = file;
            this.TaskName = taskName;
        }
    }
}