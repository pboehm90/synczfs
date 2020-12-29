using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using synczfs.CommonObjects;

namespace synczfs.processhelper
{
    public class ShellProcess
    {
        public List<string> StandardOutputLines { get; }
        public List<string> StandardErrorLines { get; }
        Thread ProcessThread { get; set; }
        CancellationTokenSource CTS { get; }
        public bool Finished => ExitCode != -1;
        string Command { get; set; }
        public int ExitCode { get; private set; }

        public static ShellProcess RunNew(Target target, string command)
        {
            ShellProcess proc = new ShellProcess();

            if (target != null && target.UseSsh)
                command = $"ssh {target.Username}@{target.Host} '{command}'";

            proc.Run(command);
            return proc;
        }

        private ShellProcess()
        {
            ExitCode = -1;
            StandardOutputLines = new List<string>();
            StandardErrorLines = new List<string>();
            CTS = new CancellationTokenSource();
        }

        private void Run(string command)
        {
            Command = command;

            ProcessThread = new Thread(RunThread);
            ProcessThread.Start();
        }

        public void Cancel()
        {
            CTS.Cancel();
            WaitForExit();
        }

        private void RunThread()
        {
            var escapedArgs = Command.Replace("\"", "\\\"");
            
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
            
            process.OutputDataReceived += ProcessOutputDataReceived;
            process.ErrorDataReceived += ProcessErrorDataReceived;
            
            process.Start();
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Logging.GetInstance().Log("Prozess gestartet! " + process.StartInfo.Arguments); 
            
            while (!process.HasExited)
            {
                if (CTS.IsCancellationRequested)
                {
                    process.Kill();
                    break;
                }
                Thread.Sleep(50);
            }

            if (process.HasExited)
                ExitCode = process.ExitCode;
        }

        public ShellProcess WaitForExit()
        {
            ProcessThread.Join();
            if (ExitCode != 0)
                throw new ProcessException(ExitCode, Command, GetStandardError());
            Logging.GetInstance().Log($"Prozess erfoglreich beendet!");
            return this;
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Append(e.Data, StandardOutputLines);
        }

        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Append(e.Data, StandardErrorLines);
        }

        private void Append(string data, List<string> lines)
        {
            if (!string.IsNullOrEmpty(data))
                lines.Add(data);
        }

        private string ListToString(List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                sb.Append(lines[i]);
                if (i + 1 < lines.Count)
                    sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        public string GetStandardOutput()
        {
            return ListToString(StandardOutputLines);
        }

        public string GetStandardError()
        {
            return ListToString(StandardErrorLines);
        }
    }
}