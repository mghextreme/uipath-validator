namespace UIPathValidator.UIPath
{
    public class Argument : Variable
    {
        public ArgumentDirection Direction { get; set; }

        public Argument() : base() { }

        public Argument(string name) : base(name) { }

        public Argument(string name, ArgumentDirection direction) : base(name)
        {
            Direction = direction;
        }

        public Argument(string name, ArgumentDirection direction, string type) : base(name, type)
        {
            Direction = direction;
        }

        public static Argument CreateFromAttributes(string name, string type)
        {
            Argument arg = new Argument(name);

            int parenthesisIndex = type.IndexOf('(');

            string dirType = type.Substring(0, parenthesisIndex);
            ArgumentDirection argDirection = ArgumentDirectionMethods.Parse(dirType);
            arg.Direction = argDirection;

            arg.Type = type.Substring(parenthesisIndex + 1, type.Length - parenthesisIndex - 2);
            
            return arg;
        }
    }
}