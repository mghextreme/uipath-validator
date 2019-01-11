using System;
using UIPathValidator;
using UIPathValidator.UIPath;
using UIPathValidator.Validation;

namespace UIPathValidator.CLI
{
    class Program
    {
        const string ProjectFolder = @"C:\Projects\Capgemini\HRO\HelpDesk2.0\Automation";

        static void Main(string[] args)
        {
            Project project = new Project(ProjectFolder);
            ProjectValidator validator = new ProjectValidator(project);
            
            Console.WriteLine("Loading...");
            project.Load();

            Console.WriteLine("Starting validation on project {0}.", project.Name);
            validator.Validate();
            Console.WriteLine("Validation finished. Printing results.");
            Console.WriteLine();

            var colorBg = Console.BackgroundColor;
            var colorFore = Console.ForegroundColor;

            Console.BackgroundColor = ConsoleColor.Black;

            WriteTypeCount(validator, ValidationResultType.Error);
            WriteTypeCount(validator, ValidationResultType.Warning);
            WriteTypeCount(validator, ValidationResultType.Info);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;

            foreach (var result in validator.GetResults())
            {
                WriteValidationResult(result);
            }

            Console.BackgroundColor = colorBg;
            Console.ForegroundColor = colorFore;
        }

        private static void WriteTypeCount(ProjectValidator validator, ValidationResultType type)
        {
            int count = validator.CountType(type);
            var color = GetColorFromResultType(type);
            
            if (count > 0)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("{0} {1} messages.", count, type.ToString());
            }
        }

        private static void WriteValidationResult(ValidationResult resultItem)
        {
            Console.ForegroundColor = GetColorFromResultType(resultItem.Type);
            Console.Write(resultItem.Type.ToString().ToUpper() + ": ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(resultItem.FormattedMessage);
        }

        private static ConsoleColor GetColorFromResultType(ValidationResultType type)
        {
            switch (type)
            {
                case ValidationResultType.Error: return ConsoleColor.Red;
                case ValidationResultType.Warning: return ConsoleColor.Yellow;
                case ValidationResultType.Info: return ConsoleColor.Blue;
            }
            return ConsoleColor.White;
        }
    }
}
