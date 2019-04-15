using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class EmptyIfReferee : IWorkflowReferee
    {
        public string Code => "empty-if";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var ifTags = reader.Document.Descendants(XName.Get("If", reader.Namespaces.DefaultNamespace));

            foreach (var ifTag in ifTags)
            {
                if (ifTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var ifThenTag = ifTag.Elements(XName.Get("If.Then", reader.Namespaces.DefaultNamespace));
                var ifElseTag = ifTag.Elements(XName.Get("If.Else", reader.Namespaces.DefaultNamespace));

                if (ifThenTag.Count() == 0 &&
                    ifElseTag.Count() == 0)
                {
                    var name = ifTag.Attribute("DisplayName")?.Value ?? "If";
                    var message = "If activity has no activities inside.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
            }

            return results;
        }
    }
}