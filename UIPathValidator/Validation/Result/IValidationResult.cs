namespace UIPathValidator.Validation.Result
{
    public interface IValidationResult
    {
        ValidationResultType Type { get; set; }
        string Message { get; set; }
        string ToString();
    }
}