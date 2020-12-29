using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace synczfs
{
    public class Logging : IDisposable
    {
        static object initLock = new object();
        static Logging instance;
        List<string> LogStack { get; }
        public static Logging GetInstance()
        {
            lock (initLock)
            {
                if (instance == null)
                    instance = new Logging();
            }
            return instance;
        }
        
        private Logging()
        {
            LogStack = new List<string>();
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
            string logDir = Path.Combine(currentDir, "logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
            return logDir;
        }
        
        private string ToLogString(string textLine)
        {
            string dateStr = DateTime.Now.ToString("HH:mm:ss");
            string logText = $"[{dateStr}] " + textLine;

            return logText;
        }

        public void Log(string text)
        {
            LogStack.Add(ToLogString(text));
            Console.WriteLine(text);
        }

        public void FlushToFile(string jobName)
        {
            File.WriteAllLines(GetLogfile(jobName), LogStack);
        }

        public void Dispose()
        {
            LogStack.Clear();
            instance = null;
        }
    }
}