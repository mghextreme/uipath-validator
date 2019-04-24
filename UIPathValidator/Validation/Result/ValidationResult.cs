namespace UIPathValidator.Validation.Result
{
    public class ValidationResult : IValidationResult
    {
        public ValidationResultType Type { get; set; }

        public string Message { get; set; }
        
        public virtual string FormattedMessage { get { return Message; } }

        public ValidationResult() { }

        public ValidationResult(string message)
        {
            Type = ValidationResultType.Info;
            Message = message;
        }

        public ValidationResult(ValidationResultType type)
        {
            Type = type;
        }

        public ValidationResult(string message, ValidationResultType type) : this(message)
        {
            Type = type;
        }
        
        public override string ToString() => string.Format("{0}: {1}", Type.ToString().ToUpper(), FormattedMessage);
    }
}