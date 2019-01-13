using System;
using CommandLine;

namespace UIPathValidator.CLI
{
    class Program
    {
        static int Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<ValidateVerb>(args);
            int exitCode = result.MapResult(
                validate => validate.Execute(),
                err => 1
            );
            return exitCode;
        }
    }
}
