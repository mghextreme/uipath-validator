using System.Collections.Generic;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class CommentOutReferee : IWorkflowReferee
    {
        public string Code => "comment-out";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();

            if (!reader.Namespaces.HasNamespace("ui"))
                return results;

            var commentOutTags = reader.Document.Descendants(XName.Get("CommentOut", reader.Namespaces.LookupNamespace("ui")));

            foreach (var commentOut in commentOutTags)
            {
                var name = commentOut.Attribute("DisplayName")?.Value ?? "CommentOut";
                var message = "CommentOut activities should be removed from workflow.";
                results.Add(new CommentOutValidationResult(workflow, name, ValidationResultType.Info, message));
            }

            return results;
        }
    }
}