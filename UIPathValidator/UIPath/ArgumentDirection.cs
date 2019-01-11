using System;

namespace UIPathValidator.UIPath
{
    public enum ArgumentDirection
    {
        In,
        InOut,
        Out
    }

    public static class ArgumentDirectionMethods
    {
        public static string Prefix(this ArgumentDirection type)
        {
            switch (type)
            {
                case ArgumentDirection.In:
                    return "in_";
                case ArgumentDirection.InOut:
                    return "io_";
                case ArgumentDirection.Out:
                    return "out_";
            }
            return string.Empty;
        }

        public static ArgumentDirection Parse(string argument)
        {
            switch (argument.ToLower())
            {
                case "in":
                case "inargument":
                    return ArgumentDirection.In;
                case "io":
                case "inout":
                case "inoutargument":
                    return ArgumentDirection.InOut;
                case "out":
                case "outargument":
                    return ArgumentDirection.Out;
            }
            throw new ArgumentException(string.Format("The argument direction {0} could not be parsed.", argument), "argument");
        }
    }
}