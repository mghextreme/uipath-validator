using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class EmptyWhileReferee : IWorkflowReferee
    {
        public string Code => "empty-while";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var whileTags = reader.Document.Descendants(XName.Get("While", reader.Namespaces.DefaultNamespace));

            foreach (var whileTag in whileTags)
            {
                if (whileTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var insideTags = whileTag.Elements();

                if (insideTags.Count() == 0)
                {
                    var name = whileTag.Attribute("DisplayName")?.Value ?? "While";
                    var message = "While activity has no activities inside.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
            }

            return results;
        }
    }
}