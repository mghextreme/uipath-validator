using System;
using UIPathValidator;
using UIPathValidator.UIPath;

namespace UIPathValidator.CLI
{
    class Program
    {
        const string ProjectFolder = @"E:\Matias\Documents\UiPath\Example";

        static void Main(string[] args)
        {
            Project project = new Project(ProjectFolder);
            ProjectValidator validator = new ProjectValidator(project);
            validator.Validate();
        }
    }
}
