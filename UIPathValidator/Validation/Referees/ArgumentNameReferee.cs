using System.Collections.Generic;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
        
    public class ArgumentNameReferee : IWorkflowReferee
    {
        public string Code => "argument-name";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            foreach (var argument in workflow.Arguments)
            {
                if (!argument.Name.StartsWith(argument.Direction.Prefix()))
                {
                    results.Add(new ArgumentValidationResult(argument.Name, workflow, ValidationResultType.Warning, $"{argument.Direction.ToString()}Argument doesn't start with prefix '{argument.Direction.Prefix()}'."));
                }
                else
                {
                    var underscorePos = argument.Name.IndexOf('_');
                    var name = argument.Name.Substring(underscorePos + 1);

                    if (name.ContainsAccents())
                        results.Add(new ArgumentValidationResult(argument.Name, workflow, ValidationResultType.Warning, $"Argument contains invalid non-ASCII characters."));
                    
                    if (!name[0].IsUppercaseLetter())
                        results.Add(new ArgumentValidationResult(argument.Name, workflow, ValidationResultType.Warning, $"Argument doesn't start with a capital letter."));
                }
            }

            return results;
        }
    }
}