using System;
using System.Collections.Generic;
using System.Diagnostics;
using synczfs.CommonObjects;

namespace synczfs.processhelper
{
    public class SimpleShellProcess
    {
        string Command { get; }
        public string StandardOutput { get; }
        public string[] StandardOutputLines { get; private set; }

        public static SimpleShellProcess Run(Target target, string cmd)
        {
            return new SimpleShellProcess(target, cmd);
        }

        private SimpleShellProcess(Target target, string cmd)
        {
            if (target != null && target.UseSsh)
                cmd = $"ssh {target.Username}@{target.Host} '{cmd}'";
            
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
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
                Logging.GetInstance().Log("Prozess gestartet. " + fullExecString); 
                process.WaitForExit();
                Logging.GetInstance().Log("Prozess beendet! " + fullExecString); 

                if (process.ExitCode == 0)
                {
                    string stdOut = process.StandardOutput.ReadToEnd();
                    // Zeilenumbruch ganz am Ende entfernen, macht das parsen leichter!
                    if (stdOut.EndsWith(Environment.NewLine))
                        stdOut = stdOut.Substring(0, stdOut.Length - Environment.NewLine.Length);
                    
                    StandardOutput = stdOut;
                    StandardOutputLines = StandardOutput.Split(Environment.NewLine, StringSplitOptions.None);
                }
                else
                {
                    string stdErr = process.StandardError.ReadToEnd();
                    throw new ProcessException(process.ExitCode, fullExecString, stdErr);
                }
            }
            finally
            {
                process?.Dispose();
                process = null;
            }
        }
    }
}