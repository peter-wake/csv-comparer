using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine
{
    public class CommandLineParser
    {
        /// <summary>
        /// A delegate to match option texts in a list and process them.
        /// Modifies the optionTexts list, removing processed texts.
        /// </summary>
        /// <param name="optionTexts">The option texts collection to process.</param>
        public delegate void OptionMatcher(List<string> optionTexts);

        public string ProgramName { get; private set; }

        public CommandLineParser(string commandLine)
        {
            _commandLine = commandLine;
        }

        /// <summary>
        /// Parses a command line string set at construction by applying a collection of matchers.
        /// The matchers execute arbitrary code to process the option state.
        /// </summary>
        /// <param name="matchers">The collection of matcher delegates to use.</param>
        /// <returns>The list of unparsed "remainder" options that can be reported as errors.</returns>
        public List<string> Parse(ICollection<OptionMatcher> matchers)
        {
            // The command line string is pre-split according to rules that are independent of the matchers.
            // The line is broken when whitespace occurs, unless a quote is encountered, in which case processing runs to the next quote.
            // Unless the next quote is pre-escaped with a \ character.
            // Whitespace outside quotes is discarded and stripped from option strings.
            // Whitespace inside quotes is preserved verbatim and does not cause a break.
            var stringCollection = SplitString(_commandLine);

            ProgramName = stringCollection.FirstOrDefault();
            if (null != ProgramName)
            {
                stringCollection.RemoveAt(0);
            }

            foreach (var matcher in matchers)
            {
                matcher(stringCollection);
            }

            return stringCollection;
        }

        public static List<string> SplitString(string splitMe)
        {
            const char backslash = '\\';
            const char quote = '"';
            var splitStrings = new List<string>();
            var accumulator = new StringBuilder();
            bool inQuotes = false;
            bool eatingWhitespace = true;
            bool pendingEscape = false;

            foreach (char cc in splitMe)
            {
                if (inQuotes)
                {
                    if (backslash == cc)
                    {
                        if (pendingEscape)
                        {
                            accumulator.Append(backslash);
                            pendingEscape = false;
                            continue;
                        }
                        pendingEscape = true;
                    }
                    else if (quote == cc)
                    {
                        if (pendingEscape)
                        {
                            accumulator.Append(quote);
                            pendingEscape = false;
                            continue;
                        }
                        inQuotes = false;
                    }
                    else
                    {
                        if (pendingEscape)
                        {
                            accumulator.Append(backslash);
                            pendingEscape = false;
                        }
                        accumulator.Append(cc);
                    }
                }
                else
                {
                    if (char.IsWhiteSpace(cc))
                    {
                        if (pendingEscape)
                        {
                            accumulator.Append(cc);
                            pendingEscape = false;
                            continue;
                        }

                        if (!eatingWhitespace)
                        {
                            // End of build
                            splitStrings.Add(accumulator.ToString());
                            accumulator.Clear();
                            eatingWhitespace = true;
                        }
                    }
                    else
                    {
                        if (backslash == cc)
                        {
                            eatingWhitespace = false;
                            if (pendingEscape)
                            {
                                accumulator.Append(backslash);
                                pendingEscape = false;
                                continue;
                            }
                            pendingEscape = true;
                        }
                        else if (quote == cc)
                        {
                            if (pendingEscape)
                            {
                                accumulator.Append(quote);
                                pendingEscape = false;
                                continue;
                            }
                            inQuotes = true;
                            // quote begins new symbol, like whitespace, but we don't eat whitespace afterwards because there hasn't been any
                            if (!eatingWhitespace)
                            {
                                splitStrings.Add(accumulator.ToString());
                                accumulator.Clear();
                            }
                            eatingWhitespace = false;
                        }
                        else 
                        {
                            eatingWhitespace = false;
                            if (pendingEscape)
                            {
                                accumulator.Append(backslash);
                                pendingEscape = false;
                            }
                            accumulator.Append(cc);
                        }
                    }
                }
            }

            if (accumulator.Length > 0)
            {
                splitStrings.Add(accumulator.ToString());
            }

            return splitStrings;
        }

        private readonly string _commandLine;
    }
}