using UIPathValidator.UIPath;

namespace UIPathValidator.Validation
{
    internal class VariableValidationResult : ValidationResult
    {
        public Variable Variable { get; set; }
        public Workflow Workflow { get; set; }

        public override string FormattedMessage
        {
            get
            {
                string varName = Variable.Name;
                if (this.Variable is ActivityVariable actVar)
                {
                    if (!string.IsNullOrWhiteSpace(actVar.Context))
                        varName += $" ({actVar.Context})";
                }

                return string.Format("{0} - {1}: {2}", Workflow.RelativePath, varName, Message);
            }
        }

        public VariableValidationResult(Variable variable) : base(ValidationResultType.Warning)
        {
            this.Variable = variable;
        }

        public VariableValidationResult(Variable variable, Workflow workflow) : base(ValidationResultType.Warning)
        {
            this.Variable = variable;
            Workflow = workflow;
        }

        public VariableValidationResult(Variable variable, Workflow workflow, ValidationResultType type) : base(type)
        {
            this.Variable = variable;
            Workflow = workflow;
        }

        public VariableValidationResult(Variable variable, Workflow workflow, ValidationResultType type, string message) : base(message, type)
        {
            this.Variable = variable;
            Workflow = workflow;
        }
    }
}