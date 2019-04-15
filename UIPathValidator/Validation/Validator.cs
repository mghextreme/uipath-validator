using System.Collections.Generic;
using System.Linq;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation
{
    public abstract class Validator
    {
        protected List<ValidationResult> Results { get; set; }

        public Validator()
        {
            Results = new List<ValidationResult>();
        }

        public abstract void Validate();

        public void AddResult(ValidationResult result)
        {
            Results.Add(result);
        }

        public IEnumerable<ValidationResult> GetResults()
        {
            return new List<ValidationResult>(Results.ToArray());
        }

        public IEnumerable<ValidationResult> GetResultsByType(ValidationResultType type)
        {
            return
                (from item in Results
                    where item.Type == type
                select item);
        }

        public int Count()
        {
            return Results.Count;
        }

        public int CountByType(ValidationResultType type)
        {
            return GetResultsByType(type).Count();
        }
    }
}