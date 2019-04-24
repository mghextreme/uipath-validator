using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UIPathValidator.UIPath;
using UIPathValidator.Validation.Result;

namespace UIPathValidator.Validation.Referees
{
    public class DelayReferee : IWorkflowReferee
    {
        public string Code => "delay";

        private Regex timeRegex;
        private CultureInfo enCulture;

        public DelayReferee()
        {
            timeRegex = new Regex(@"([0-9]+):([0-9]+):([0-9]+)(\.[0-9+]+)?");
            enCulture = CultureInfo.GetCultureInfo("en-US");
        }

        public ICollection<ValidationResult> Validate(Workflow workflow)
        {
            var results = new List<ValidationResult>();

            FillActivitiesDelay(workflow);
            FillAttributesDelay(workflow);

            if (workflow.DelayTotal > 1)
            {
                string message = "Total coded delay is of {0} seconds ({1}s in Delay activities and {2}s in attributes).";
                message = string.Format(message, workflow.DelayTotal.ToString("F1"), workflow.DelayOnActivities.ToString("F1"), workflow.DelayOnAttributes.ToString("F1"));
                results.Add(new DelayValidationResult(workflow, ValidationResultType.Info, message));
            }

            return results;
        }

        private void FillActivitiesDelay(Workflow workflow)
        {
            var reader = workflow.GetXamlReader();
            var delayActivities = reader.Document.Descendants(XName.Get("Delay", reader.Namespaces.DefaultNamespace));

            workflow.DelayOnActivities = 0;
            foreach (var delay in delayActivities)
            {
                if (delay.IsInsideCommentOut(reader.Namespaces))
                    continue;

                var durationString = delay.Attribute("Duration")?.Value ?? "00:00:00";
                var duration = DurationStringToSeconds(durationString);
                if (duration > 0)
                    workflow.DelayOnActivities += duration;
            }
        }

        private void FillAttributesDelay(Workflow workflow)
        {
            var reader = workflow.GetXamlReader();
            var delayAttributeTags =
                from el in reader.Document.Descendants()
                where
                    el.Attribute("DelayBefore") != null ||
                    el.Attribute("DelayMS") != null
                select el;

            workflow.DelayOnAttributes = 0;
            foreach (var delay in delayAttributeTags)
            {
                if (delay.IsInsideCommentOut(reader.Namespaces))
                    continue;

                string delayBeforeStr = delay.Attribute("DelayBefore")?.Value ?? string.Empty,
                       delayAfterStr = delay.Attribute("DelayMS")?.Value ?? string.Empty;
                int delayBefore = 0,
                    delayAfter = 0;

                if (!string.IsNullOrWhiteSpace(delayBeforeStr) && delayBeforeStr[0] != '[' && delayBeforeStr[0] != '{')
                    delayBefore = int.Parse(delayBeforeStr);
                if (!string.IsNullOrWhiteSpace(delayAfterStr) && delayAfterStr[0] != '[' && delayAfterStr[0] != '{')
                    delayAfter = int.Parse(delayAfterStr);
                workflow.DelayOnAttributes += (decimal)(delayBefore + delayAfter) / 1000;
            }
        }

        private decimal DurationStringToSeconds(string timeString)
        {
            var match = timeRegex.Match(timeString);
            if (match.Success)
            {
                var hours = int.Parse(match.Groups[1].Value);
                var minutes = int.Parse(match.Groups[2].Value);
                var seconds = int.Parse(match.Groups[3].Value);
                var millis = decimal.Parse("0" + match.Groups[4].Value, enCulture);
                return (3600 * hours) + (60 * minutes) + seconds + millis;
            }
            return 0;
        }
    }
}