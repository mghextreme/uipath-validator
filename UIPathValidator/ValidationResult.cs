namespace UIPathValidator
{
    public class ValidationResult
    {
        public ValidationResultType Type { get; set; }

        public string Message { get; set; }
        
        public int File { get; set; }

        public int Line { get; set; }

        public ValidationResult(string message)
        {
            Type = ValidationResultType.Info;
            Message = message;
        }

        public ValidationResult(string message, ValidationResultType type) : this(message)
        {
            Type = type;
        }
    }
}