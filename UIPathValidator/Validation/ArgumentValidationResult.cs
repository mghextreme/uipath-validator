using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    public class ArgumentValidationResult : ValidationResult
    {
        public string ArgumentName { get; set; }
        public Workflow Workflow { get; set; }

        public override string FormattedMessage
        {
            get
            {
                return string.Format("{0} - {1}: {2}", Workflow.RelativePath, ArgumentName, Message);
            }
        }

        public ArgumentValidationResult(string argumentName) : base(ValidationResultType.Warning)
        {
            ArgumentName = argumentName;
        }

        public ArgumentValidationResult(string argumentName, Workflow workflow) : base(ValidationResultType.Warning)
        {
            ArgumentName = argumentName;
            Workflow = workflow;
        }

        public ArgumentValidationResult(string argumentName, Workflow workflow, ValidationResultType type) : base(type)
        {
            ArgumentName = argumentName;
            Workflow = workflow;
        }

        public ArgumentValidationResult(string argumentName, Workflow workflow, ValidationResultType type, string message) : base(message, type)
        {
            Workflow = workflow;
            ArgumentName = argumentName;
        }
    }
}