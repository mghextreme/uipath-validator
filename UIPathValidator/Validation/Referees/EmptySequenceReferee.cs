using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class EmptySequenceReferee : IWorkflowReferee
    {
        public string Code => "empty-sequence";

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            var reader = workflow.GetXamlReader();
            var sequenceTags = reader.Document.Descendants(XName.Get("Sequence", reader.Namespaces.DefaultNamespace));

            foreach (var sequenceTag in sequenceTags)
            {
                if (sequenceTag.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var insideTags = sequenceTag.Elements();

                if (insideTags.Count() == 0)
                {
                    var name = sequenceTag.Attribute("DisplayName")?.Value ?? "Sequence";
                    var message = "Sequence activity has no activities inside.";
                    results.Add(new EmptyScopeValidationResult(workflow, name, ValidationResultType.Warning, message));
                }
            }

            return results;
        }
    }
}