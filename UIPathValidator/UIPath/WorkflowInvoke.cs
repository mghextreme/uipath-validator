using System.Collections.Generic;

namespace UIPathValidator.UIPath
{
    public class WorkflowInvoke
    {
        public string Name { get; protected set; }

        public Workflow Workflow { get; protected set; }

        public List<Argument> Arguments { get; protected set; }

        public WorkflowInvoke(Workflow workflow)
        {
            Arguments = new List<Argument>();
            this.Workflow = workflow;
        }

        public WorkflowInvoke(Workflow workflow, string name) : this(workflow)
        {
            Name = name;
        }

        public void AddArgument(Argument argument)
        {
            Arguments.Add(argument);
        }
    }
}