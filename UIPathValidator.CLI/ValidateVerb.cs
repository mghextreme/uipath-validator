using System;
using System.Linq;
using CommandLine;
using UIPathValidator.UIPath;
using UIPathValidator.Validation;

namespace UIPathValidator.CLI
{
    [Verb("validate", HelpText = "Check for validation in a project.")]
    public class ValidateVerb
    {
        [Option('p', "project", HelpText = "The project folder or project json file.", Required = true)]
        public string Project { get; set; }

        [Option("hide-info", HelpText = "If should hide the results of type Info.", Default = false)]
        public bool HideInfo { get; set; }

        [Option("hide-warning", HelpText = "If should hide the results of type Warning.", Default = false)]
        public bool HideWarning { get; set; }

        [Option("hide-error", HelpText = "If should hide the results of type Error.", Default = false)]
        public bool HideError { get; set; }

        protected ConsoleColor foregroundColor = ConsoleColor.Gray,
                                infoColor = ConsoleColor.Blue,
                                warningColor = ConsoleColor.Yellow,
                                errorColor = ConsoleColor.Red;

        public int Execute()
        {
            Project project = new Project(Project);
            ProjectValidator validator = new ProjectValidator(project);

            ConsoleColor foregroundBackup = Console.ForegroundColor;
            ConsoleColor backgroundBackup = Console.BackgroundColor;

            switch (backgroundBackup)
            {
                case ConsoleColor.Black: break;
                case ConsoleColor.Blue:
                case ConsoleColor.DarkBlue:
                    foregroundColor = ConsoleColor.White;
                    infoColor = ConsoleColor.Cyan;
                    break;
                default:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = foregroundColor;
                    break;
            }

            Console.WriteLine("Loading info...");
            project.Load();

            Console.WriteLine("Starting validation on project {0}.", project.Name);
            validator.Validate();
            Console.WriteLine("Validation finished. Printing results.");

            if (!HideError)
                PrintResultsOfType(validator, ValidationResultType.Error);
                
            if (!HideWarning)
                PrintResultsOfType(validator, ValidationResultType.Warning);
                
            if (!HideInfo)
                PrintResultsOfType(validator, ValidationResultType.Info);
            
            Console.ForegroundColor = foregroundColor;
            
            Console.WriteLine();
            Console.WriteLine("Total number of situations found: {0}", validator.Count());
            
            Console.ForegroundColor = foregroundBackup;
            Console.BackgroundColor = backgroundBackup;

            return 0;
        }

        private void PrintResultsOfType(ProjectValidator validator, ValidationResultType type)
        {
            var filteredResults = validator.GetResultsByType(type);
            Console.WriteLine();
            WriteTypeCount(type, filteredResults.Count());

            foreach (var item in filteredResults)
                WriteValidationResult(item);
        }

        private void WriteTypeCount(ValidationResultType type, int count)
        {
            if (count > 0)
            {
                Console.ForegroundColor = GetColorFromResultType(type);
                Console.WriteLine("{0} {1} messages:", count, type.ToString());
            }
        }

        private void WriteValidationResult(ValidationResult resultItem)
        {
            Console.ForegroundColor = GetColorFromResultType(resultItem.Type);
            Console.Write(resultItem.Type.ToString().ToUpper() + ": ");
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(resultItem.FormattedMessage);
        }

        private ConsoleColor GetColorFromResultType(ValidationResultType type)
        {
            switch (type)
            {
                case ValidationResultType.Error: return errorColor;
                case ValidationResultType.Warning: return warningColor;
                case ValidationResultType.Info: return infoColor;
            }
            return ConsoleColor.White;
        }
    }
}