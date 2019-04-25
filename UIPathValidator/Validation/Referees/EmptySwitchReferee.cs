using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class EmptySwitchReferee : IWorkflowReferee
    {
        public string Code => "empty-while";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var switchTags = reader.Document.Descendants(XName.Get("Switch", reader.Namespaces.DefaultNamespace));

            foreach (var switchTag in switchTags)
            {
                if (switchTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var insideTags = switchTag.Elements();

                if (insideTags.Count() <= 1)
                {
                    var name = switchTag.Attribute("DisplayName")?.Value ?? "Switch";
                    var message = "Switch activity only has default case.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }

                var xNamespace = reader.Namespaces.LookupNamespace("x");
                foreach (var subTag in insideTags)
                {
                    if (subTag.Name == XName.Get("Null", xNamespace))
                    {
                        var name = switchTag.Attribute("DisplayName")?.Value ?? "Switch";
                        var caseName = subTag.Attribute(XName.Get("Key", xNamespace))?.Value ?? "undefined";
                        var message = string.Format("Switch activity case {0} has no activities inside.", caseName);
                        results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                    }
                }
            }

            return results;
        }
    }
}