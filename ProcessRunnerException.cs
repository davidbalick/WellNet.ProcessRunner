using System;

namespace WellNet.ProcessRunner
{
    public class ProcessRunnerException : Exception
    {
        public string Context { get; set; }

        public ProcessRunnerException(string context, string message) : base(message)
        {
            Context = context;
        }
    }
}
