using System.Collections.Generic;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public interface IProjectReferee
    {
        string Code { get; }

        ICollection<ValidationResult> Validate(Project project);
    }
}