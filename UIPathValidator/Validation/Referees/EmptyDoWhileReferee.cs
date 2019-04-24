using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class EmptyDoWhileReferee : IWorkflowReferee
    {
        public string Code => "empty-dowhile";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var doWhileTags = reader.Document.Descendants(XName.Get("DoWhile", reader.Namespaces.DefaultNamespace));

            foreach (var doWhileTag in doWhileTags)
            {
                if (doWhileTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var insideTags = doWhileTag.Elements();

                if (insideTags.Count() == 0)
                {
                    var name = doWhileTag.Attribute("DisplayName")?.Value ?? "Do While";
                    var message = "Do While activity has no activities inside.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
            }

            return results;
        }
    }
}