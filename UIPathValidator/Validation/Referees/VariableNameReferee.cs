using System.Collections.Generic;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class VariableNameReferee : IWorkflowReferee
    {
        public string Code => "variable-name";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            foreach (var variable in workflow.Variables)
            {
                if (variable.Name.ContainsAccents())
                    results.Add(new VariableValidationResult(variable, workflow, ValidationResultType.Warning, $"Variable contains invalid non-ASCII characters."));

                if (variable.Name[0].IsUppercaseLetter())
                    results.Add(new VariableValidationResult(variable, workflow, ValidationResultType.Warning, $"Variable doesn't start with a lowercase letter."));
            }

            return results;
        }
    }
}