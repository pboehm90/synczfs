using System;

namespace synczfs.processhelper
{
    class ProcessException : Exception
    {
        public int ExitCode { get; }
        public string Error { get; }
        public string Command { get; }
        public ProcessException(int exitCode, string command, string error) : base($"Process returned Error {exitCode}!" + Environment.NewLine + "Command: [" + command + "] " + Environment.NewLine + "StdErr Output: "+ error)
        {
            ExitCode = exitCode;
            Error = error;
            Command = command;
        }
    }
}