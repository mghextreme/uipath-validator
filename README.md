# UIPath Validator

This is a project currently being built to ensure that an UIPath Project is using the best practices.

The application will run on a specified folder and check for many inconsistences in the project, returning them as a list. Below you can find what are the current validations.

## How to use it

Open the solution and make sure to publish it:

`Ctrl + Shift + P > Tasks: Run Task > publish`

After published, you may open the project folder for the `UIPathValidator.CLI`, navigate to the folder containing the `.exe` (probably `\bin\Release\netcoreapp2.1\win10-x64`) and run the following command:

`UIPathValidator.CLI.exe validate -p <your-project-folder>`

## Validations

- Variables
  - Names should start with a capital letter (CamelCase)
  - Names should not contain accents
- Arguments
  - Names should start with direction prefix (e.g. in_)
  - Names should start with a capital letter after the underscore
  - Names should not contain accents
- Invoke Workflow
  - Invoked workflow file should exist
  - All workflow arguments should be present
  - No spare arguments should be present
  - Arguments should have the same type and direction
- Empty scopes
  - Flowchart activities should have at least one activity inside
  - Sequence activities should have at least one activity inside
  - While activities should have at least one activity inside
  - Do While activities should have at least one activity inside
  - If activities should have at least one activity on either then or else
- Flowchart
  - Should not have any disconnected / orphans activities
  - Suggests that Flowchart with no decisions or switches should be sequences

## TODO

- Ignore workflows errors when inside CommentOut
- Check for files not being used
- Try Catch should have catches
- Switches need cases