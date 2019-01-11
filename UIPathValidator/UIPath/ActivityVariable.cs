namespace UIPathValidator.UIPath
{
    public class ActivityVariable : Variable
    {
        public string Context { get; set; }

        public ActivityVariable() : base() { }

        public ActivityVariable(string name) : base(name) { }

        public ActivityVariable(string name, string type) : base(name, type) { }

        public ActivityVariable(string name, string type, string context) : base(name)
        {
            Context = context;
        }
    }
}