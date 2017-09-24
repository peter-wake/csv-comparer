using System.Collections.Generic;
using System.IO;
using CommandLine;

namespace CsvCompare
{
    internal class CommandLineArguments : CommandLineArgumentsBase
    {
        private const int ExpectedArgumentCount = 2;

        private const string SkipLineFlag = "-s";
        private const string TrimWhitespaceFlag = "-t";

        public int SkipLineCount { get; private set; }

        public bool TrimWhitespace { get; private set; }

        public string FileNameLeft { get; private set; }

        public string FileNameRight { get; private set; }

        private bool ShowHelp { get; set; }


        public CommandLineArguments()
        {
            SkipLineCount = 1;
            TrimWhitespace = false;

            FileNameLeft = null;
            FileNameRight = null;
            ShowHelp = false;

            Matchers = new List<CommandLineParser.OptionMatcher>()
            {
                SkipLineMatcher,
                TrimLineMatcher,
                HelpMatcher,
                Finish
            };
        }

        public override string GetHelp()
        {
            return $"usage: CsvCompare [{SkipLineFlag} <skip-line-count>] [{TrimWhitespaceFlag}] [-?|-h|--help] <left-file-name> <right-file-name>\n" +
                   $"    {SkipLineFlag} <line-count>   :  Skip the specified number of initial lines before comparison (defaults to 1)\n" +
                   $"    {TrimWhitespaceFlag}                :  trim whitespace\n" +
                   $"    -? | -h | --help  :  show this help\n";
        }

        private void HelpMatcher(List<string> arguments)
        {
            FindFlag("-?", arguments, () => { ShowHelp = true; });
            FindFlag("-h", arguments, () => { ShowHelp = true; });
            FindFlag("--help", arguments, () => { ShowHelp = true; });
        }

        private void SkipLineMatcher(List<string> arguments)
        {
            FindParameter(SkipLineFlag,
                arguments,
                skipLineCountText =>
                {
                    uint skipLines;
                    if (uint.TryParse(skipLineCountText, out skipLines))
                    {
                        SkipLineCount = (int)skipLines;
                    }
                    else
                    {
                        Errors.Add($"SkipLineCount of {skipLineCountText} is not a valid unsigned integer.");
                    }
                });
        }

        private void TrimLineMatcher(List<string> arguments)
        {
            FindFlag(TrimWhitespaceFlag, arguments, () => { TrimWhitespace = true; });
        }

        public override void Finish(List<string> arguments)
        {
            if (ExpectedArgumentCount != arguments.Count)
            {
                Errors.Add("Incorrect number of filenames specified for comparison; there should be exactly two.");
            }
            else
            {
                FileNameLeft = arguments[0];
                FileNameRight = arguments[1];

                if (!File.Exists(FileNameLeft))
                {
                    Errors.Add($"Left file '{FileNameLeft}' does not exist at the specified location.");
                }

                if (!File.Exists(FileNameRight))
                {
                    Errors.Add($"Right file '{FileNameRight}' does not exist at the specified location.");
                }
            }

            if (ShowHelp)
            {
                throw new PrematureMatchTerminationException("  -?  -h  --help : show this help.");
            }
        }
    }
}
