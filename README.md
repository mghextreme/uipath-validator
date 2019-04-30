# UIPath Validator

This is a project currently being built to ensure that an UIPath Project is using the best practices.

The application will run on a specified folder and check for many inconsistences in the project, returning them as a list. Below you can find what are the current validations.

## How to use it

`UIPathValidator.CLI.exe validate -p <your-project-file-or-folder>`

## Compiling the code

If you are using Visual Studio, just compile the code using the Release configuration for Windows 10 (or your OS of choice).

If you are using VS Code, just open the solution and make sure to run the task to publish it:

`Ctrl + Shift + P > Tasks: Run Task > publish`

> This task will publish it by default to Windows 10 64 bits OS. If you need a different one, go into `.vscode\tasks.json` and change it.

After published, you may open the project folder for the `UIPathValidator.CLI`, navigate to the folder containing the `.exe` (probably `\bin\Release\netcoreapp2.1\win10-x64`) and run the following command:

## Validations

- Variables
  - Names should start with a lowercase letter (camelCase)
  - Names should not contain accents
- Arguments
  - Names should start with direction prefix (e.g. in_)
  - Names should start with a capital letter after the underscore (TitleCase)
  - Names should not contain accents
- Invoke Workflow
  - Invoked workflow file should exist
  - All workflow arguments should be present
  - No spare arguments should be present
  - Arguments should have the same type and direction
  - Should avoid invoke recursion (chances of loop cycles)
- Empty scopes
  - Flowchart activities should have at least one activity inside
  - Sequence activities should have at least one activity inside
  - While activities should have at least one activity inside
  - Do While activities should have at least one activity inside
  - If activities should have at least one activity on either then or else
- Flowchart
  - Should not have any disconnected / orphan activities
  - Suggests that Flowchart with no decisions or switches should be sequences
- State Machine
  - Should not have any disconnected / orphan states
  - All non-final states must have an exit and reach a final state
- Try Catch
  - Should have at least one activity in the Try section
  - Should have at least one activity in either a catch or on the finally section
- Switch
  - Should have at least one case besides Default
  - Should have at least one activity in each case
- Files
  - Files should be invoked (directly or indirectly) from the main file at least once
- Delay
  - Should avoid having delays, either as Delay activity or DelayBefore and DelayAfter attributes
- Comments
  - Should not have CommentOut activities. If the activity is not being used, it should be removed

> Validation results found within CommentOut activities blocks are not returned

## TODO

### Features

- Ignore specific files from validation
- Ignore specific referees from validation
- Logging to file
- More details on error location (line)

### Validation

- Avoid username / password variables with fixed values
- Invoked Workflow name as variable
- Many Ifs inside Ifs
- Loops inside loops
- Retry Scopes content and conditions
- State Machine validations
  - Maximum 1 empty condition
  - State Machines inside State Machines
- Workflows with delay time inside loops
- Delay activities inside loop

### Interface

- Create an easy-to-use graphical interface