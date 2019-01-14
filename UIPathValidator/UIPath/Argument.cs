using System.Xml;
using System.Xml.Linq;

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

        public static Argument CreateFromArgumentNode(XElement node, XmlNamespaceManager namespaces)
        {
            if (!namespaces.HasNamespace("x"))
                return null;

            var name = node.Attribute(XName.Get("Key", namespaces.LookupNamespace("x")))?.Value;
            var type = node.Attribute(XName.Get("TypeArguments", namespaces.LookupNamespace("x")))?.Value;
            var direction = ArgumentDirectionMethods.Parse(node.Name.LocalName);

            return new Argument(name, direction, type);
        }
    }
}