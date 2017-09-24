using System;
using Microsoft.VisualBasic.FileIO;

namespace CsvCompare
{
    class Program
    {
        const string ProgramName = "CsvCompare";

        static void Main(string[] args)
        {

            const string nullText = "NULL";

            var commandLineArguments = new CommandLineArguments();
            commandLineArguments.Parse(ProgramName);

            var fileNameLeft = commandLineArguments.FileNameLeft;
            var fileNameRight = commandLineArguments.FileNameRight;

            var parserLeft = new TextFieldParser(fileNameLeft) { TextFieldType = FieldType.Delimited };
            var parserRight = new TextFieldParser(fileNameRight) { TextFieldType = FieldType.Delimited };

            parserLeft.SetDelimiters(",");
            parserRight.SetDelimiters(",");

            var skipLines = commandLineArguments.SkipLineCount;

            parserLeft.TrimWhiteSpace = commandLineArguments.TrimWhitespace;
            parserRight.TrimWhiteSpace = commandLineArguments.TrimWhitespace;



            string message = null;
            string leftLine = null;
            string rightLine = null;

            int lineNumber = 0;
            while (!parserLeft.EndOfData)
            {
                lineNumber += 1;

                if (parserRight.EndOfData)
                {
                    message = $"Right file is shorter - out of data at line {lineNumber}";
                    break;
                }

                string[] leftFields;
                string[] rightFields;


                try
                {
                    leftFields = parserLeft.ReadFields();
                }
                catch (MalformedLineException)
                {
                    message = $"Left file malformed at line {lineNumber}";
                    break;
                }

                try
                {
                    rightFields = parserRight.ReadFields();
                }
                catch (MalformedLineException)
                {
                    message = $"Right file malformed at line {lineNumber}";
                    break;
                }

                if (lineNumber <= skipLines)
                {
                    continue;
                }



                if (leftFields.Length != rightFields.Length)
                {
                    message = $"Field counts differ at line {lineNumber}";
                    break;
                }

                var mismatched = false;
                for (int ff = 0; ff < leftFields.Length; ++ff)
                {
                    var leftField = leftFields[ff];
                    var rightField = rightFields[ff];
                    if (leftField != rightField)
                    {
                        if (string.IsNullOrEmpty(leftField) && string.IsNullOrEmpty(rightField))
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(leftField) && nullText == rightField.ToUpper())
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(rightField) && nullText == leftField.ToUpper())
                        {
                            continue;
                        }
                        mismatched = true;
                        message = $"Files differ at line {lineNumber}, field {ff+1}";
                        leftLine = string.Join(", ", leftFields);
                        rightLine = string.Join(", ", rightFields);
                        break;
                    }
                }
                if (mismatched)
                {
                    break;
                }
            }

            var outputColor = ConsoleColor.Red;
            if (null == message && !parserRight.EndOfData)
            {
                message = $"Left file is shorter - out of data at line {lineNumber}";
            }

            if (null == message)
            {
                message = $"Read {lineNumber} matching lines; files are the same";
                outputColor = ConsoleColor.Green;
            }

            Console.WriteLine();
            if (0 != skipLines)
            {
                Console.WriteLine("Skipped {0}", skipLines == 1 ? "first line" : $"{skipLines} lines");
            }

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = outputColor;
            Console.WriteLine(message);
            Console.ForegroundColor = originalColor;

            if (null != leftLine)
            {
                Console.WriteLine($"Left Line:\n{leftLine}");
            }
            if (null != rightLine)
            {
                Console.WriteLine($"Right Line:\n{rightLine}");
            }
        }


    }
}
