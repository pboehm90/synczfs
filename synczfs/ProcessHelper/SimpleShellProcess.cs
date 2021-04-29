using System;
using System.Diagnostics;
using synczfs.CommonObjects;

namespace synczfs.processhelper
{
    public class SimpleShellProcess : SimpleProcessBase
    {
        public SimpleShellProcess(Target target) : base(target)
        {
        }

        internal override ProcessResult ExecInternal(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            string stdOut = null;
            string stdErr = null;

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            string fullExecString = process.StartInfo.FileName + " " + process.StartInfo.Arguments;

            try
            {
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    stdOut = process.StandardOutput.ReadToEnd();
                }
                else
                {
                    stdErr = process.StandardError.ReadToEnd();
                }

                return new ProcessResult(process.ExitCode, stdOut, stdErr);
            }
            finally
            {
                process?.Dispose();
                process = null;
            }
        }
    }
}