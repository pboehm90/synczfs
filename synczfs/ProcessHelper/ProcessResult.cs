using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace synczfs.processhelper
{
    public class ProcessResult
    {
        public int ReturnCode { get; }
        public string[] StandardOutputLines { get; }
        public string StdErr { get; }
        public bool OK => string.IsNullOrWhiteSpace(StdErr);

        public ProcessResult(int returnCode, string stdOut, string stdErr)
        {
            ReturnCode = returnCode;

            string[] lines = stdOut.Split(Environment.NewLine);
            StandardOutputLines = new string[lines.Length - 1];
            Array.Copy(lines, StandardOutputLines, lines.Length - 1);
            
            StdErr = stdErr;
        }
    }
}