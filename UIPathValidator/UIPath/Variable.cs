using System;

namespace UIPathValidator.UIPath
{
    public class Variable
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Variable() { }

        public Variable(string name) : this()
        {
            Name = name;
        }

        public Variable(string name, string type) : this(name)
        {
            Type = type;
        }
    }
}