using System;

namespace CommandLine
{
    public class PrematureMatchTerminationException : Exception
    {
        public PrematureMatchTerminationException(string message) : base(message) { }
    }
}
