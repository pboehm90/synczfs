using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using synczfs.Exceptions;
using System.Linq;

namespace synczfs
{
    public class Logging : IDisposable
    {
        List<string> LogStack { get; }
        StreamWriter SW { get; }
        CancellationTokenSource CTS = new CancellationTokenSource();
        Thread LoopThread { get; set; }
        string LogDir { get; set; }
        public Logging(string jobName)
        {
            LogStack = new List<string>();

            try
            {
                string logfilePath = GetLogfile(jobName);
                File.WriteAllText(logfilePath, "");
                File.Delete(logfilePath);

                SW = new StreamWriter(logfilePath, false, System.Text.Encoding.UTF8);
                SW.AutoFlush = false;

                LoopThread = new Thread(LogLoop);
                LoopThread.Start();

                LogDir = Path.GetDirectoryName(logfilePath);
            }
            catch (System.Exception)
            {
                Console.WriteLine("[Error] Could not initialize the logfile!");
            }
        }

        private void LogLoop()
        {
            while (true)
            {
                int count = LogStack.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        SW.WriteLine(LogStack[i]);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        LogStack.RemoveAt(0);
                    }

                    SW.Flush();
                }

                if (CTS.IsCancellationRequested && LogStack.Count == 0)
                    break;

                Thread.Sleep(100);
            }
        }

        private string GetLogfile(string jobName)
        {
            string logDir = InitDir();
            string dateStr = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
            return Path.Combine(logDir, "log_" + jobName + "_" + dateStr + "_" + Guid.NewGuid().ToString("N").Substring(0, 5) + ".txt");
        }

        private string InitDir()
        {
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string logDir = Path.Combine("/var/log", "synczfs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            return logDir;
        }
        
        private string ToLogString(string textLine)
        {
            string dateStr = DateTime.Now.ToString("HH:mm:ss.fff");
            string logText = $"[{dateStr}] " + textLine;

            return logText;
        }

        public void Log(string text)
        {
            LogStack.Add(ToLogString(text));
            Console.WriteLine(text);
        }

        public void Log(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            if (ex is IHasExitCode)
            {
                Environment.ExitCode = ((IHasExitCode)ex).GetExitCode();
                sb.Append("ExitCode=" + Environment.ExitCode + " ");
            }
            foreach (string key in ex.Data.Keys)
            {
                sb.Append($"[{key} = {ex.Data[key]}] ");
            }

            sb.Append(ex.ToString());

            LogStack.Add(ToLogString(sb.ToString()));
            Console.Error.WriteLine(sb.ToString());
        }

        const int maxLogsizeBytes = 1024 * 1024; // 1MB Limit

        private void CleanupLogDir()
        {
            if (LogDir != null)
            {
                List<FileInfo> sortedFiles = new DirectoryInfo(LogDir).GetFiles().Where(x => x.Extension.ToLowerInvariant() == ".txt").OrderByDescending(f => f.LastWriteTime).ToList();
                long sum = 0;
                foreach (FileInfo file in sortedFiles)
                {
                    sum += file.Length;
                    if (sum >= maxLogsizeBytes)
                        file.Delete();
                }
            }
        }

        public void Dispose()
        {
            if (LoopThread != null)
            {
                CTS.Cancel();
                LoopThread.Join();
            }
            if (SW != null)
            {
                SW.Close();
                SW.Dispose();
            }
            LogStack.Clear();
            CleanupLogDir();
        }
    }
}