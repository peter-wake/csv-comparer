using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine
{
    public abstract class CommandLineArgumentsBase
    {
        private const string UnspecifiedProgramName = "UNSPECIFIED-PROGRAM-NAME";

        private const int DefaultErrorCodeValidCommandLine = 0;
        private const int DefaultErrorCodeBadCommandLine = -1;

        public IList<string> Errors { get; private set; }

        public bool HasErrors => Errors.Any();

        public IList<CommandLineParser.OptionMatcher> Matchers { get; protected set; }

        public int ErrorCodeBadCommandLine { get; set; } = DefaultErrorCodeBadCommandLine;

        public abstract string GetHelp();

        public List<string> Parse(string programName = null)
        {
            var commandLine = Environment.CommandLine;
            var parser = new CommandLineParser(commandLine);
            List<string> result = null;

            programName = programName ?? UnspecifiedProgramName;

            try
            {
                result = parser.Parse(Matchers);
            }
            catch (PrematureMatchTerminationException e)
            {
                if (HasErrors)
                {
                    DisplayErrors(programName);
                }
                else
                {
                    if (!string.IsNullOrEmpty(e.Message))
                    {
                        Console.WriteLine(e.Message);
                    }
                    Console.WriteLine("{0} {1}", programName, GetHelp());
                    Environment.Exit(DefaultErrorCodeValidCommandLine);
                }
            }

            if (HasErrors)
            {
                DisplayErrors(programName);
            }

            return result;
        }

        protected virtual void DisplayErrors(string programName)
        {
            Console.WriteLine("Command syntax-error:");
            foreach (var error in Errors)
            {
                Console.WriteLine("    {0}", error);
            }
            Console.WriteLine("Usage:");
            Console.WriteLine("{0} {1}", programName, GetHelp());
            Environment.Exit(ErrorCodeBadCommandLine);
        }

        public void BadArgumentMatcher(List<string> arguments)
        {
            int foundAt = arguments.FindLastIndex(aa => aa.StartsWith("-"));
            for (int ii = 0; ii <= foundAt; ++ii)
            {
                Errors.Add($"Unrecognized argument: '{arguments[ii]}'");
            }
            if (foundAt >= 0)
            {
                arguments.RemoveRange(0, foundAt + 1);
            }
        }

        public virtual void Finish(List<string> arguments)
        {
        }

        protected CommandLineArgumentsBase()
        {
            Errors = new List<string>();
        }

        protected bool FindFlag(string flag, List<string> arguments)
        {
            if (arguments.Contains(flag))
            {
                arguments.RemoveAll(aa => flag == aa);
                return true;
            }
            return false;
        }

        protected bool FindFlag(string flag, List<string> arguments, Action flagAction)
        {
            if (arguments.Contains(flag))
            {
                arguments.RemoveAll(aa => flag == aa);
                flagAction();
                return true;
            }
            return false;
        }

        protected bool FindParameter(string flag, List<string> arguments, out string flagParameter)
        {
            int startIndex = 0;
            int foundAt = arguments.IndexOf(flag, startIndex);
            while (foundAt >= 0)
            {
                if (foundAt < arguments.Count - 1)
                {
                    flagParameter = arguments[foundAt + 1];
                    if (!IsFlag(flagParameter))
                    {
                        arguments.RemoveRange(foundAt, 2);
                        return true;
                    }
                }
                startIndex = foundAt + 1;
                foundAt = arguments.IndexOf(flag, startIndex);
            }
            flagParameter = null;
            return false;
        }

        protected bool FindParameter(string flag, List<string> arguments, Action<string> parameterAction)
        {
            bool found = false;
            string parameter;
            while (FindParameter(flag, arguments, out parameter))
            {
                found = true;
                parameterAction(parameter);
            }

            return found;
        }

        protected static List<string> FindParameters(string flag, int requiredArguments, List<string> arguments)
        {
            List<string> foundArguments = null;
            bool failed = false;
            int startIndex = 0;

            int foundAt = arguments.IndexOf(flag, startIndex);
            while (foundAt >= 0)
            {
                foundArguments = new List<string>();

                startIndex = foundAt + 1;
                int ii;
                for (ii = 0; ii < requiredArguments; ++ii)
                {
                    int index = startIndex + ii;
                    if (index >= arguments.Count)
                    {
                        failed = true;
                        break;
                    }

                    var argument = arguments[index];
                    if (IsFlag(argument))
                    {
                        failed = true;
                        break;
                    }
                    foundArguments.Add(argument);
                }

                arguments.RemoveRange(foundAt, ii + 1);

                if (startIndex >= arguments.Count)
                {
                    break;
                }
                foundAt = arguments.IndexOf(flag, startIndex);
            }

            if (failed || null == foundArguments || foundArguments.Count != requiredArguments)
            {
                return null;
            }

            return foundArguments;
        }

        private static bool IsFlag(string parameter)
        {
            return parameter.StartsWith("-") && !IsNumber(parameter);
        }

        private static bool IsNumber(string parameter)
        {
            double number;
            return double.TryParse(parameter, out number);
        }
    }
}