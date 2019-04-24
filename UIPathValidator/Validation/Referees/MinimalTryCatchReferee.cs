using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class MinimalTryCatchReferee : IWorkflowReferee
    {
        public string Code => "minimal-try-catch";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var tryCatchTags = reader.Document.Descendants(XName.Get("TryCatch", reader.Namespaces.DefaultNamespace));

            foreach (var tryCatchTag in tryCatchTags)
            {
                if (tryCatchTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var name = tryCatchTag.Attribute("DisplayName")?.Value ?? "Do While";

                var tcTry = tryCatchTag.Element(XName.Get("TryCatch.Try", reader.Namespaces.DefaultNamespace));
                var tcCatches = tryCatchTag.Element(XName.Get("TryCatch.Catches", reader.Namespaces.DefaultNamespace));
                var tcFinally = tryCatchTag.Element(XName.Get("TryCatch.Finally", reader.Namespaces.DefaultNamespace));

                if (tcTry == null || tcTry.Elements().Count() == 0)
                {
                    var message = "Try Catch activity has no activities inside.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }

                if ((tcCatches == null || tcCatches.Elements().Count() == 0) &&
                    (tcFinally == null || tcFinally.Elements().Count() == 0))
                {
                    var message = "Try Catch activity has no catches and/or finally.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
            }

            return results;
        }
    }
}