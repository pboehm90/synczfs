using System;
using System.Collections.Generic;
using System.Text;

namespace synczfs.CommonObjects
{
    public class CliArguments
    {
        public string JobName { get; }
        public Target Source { get; }
        public Target Destination { get; }
        public bool Recursive => CliFlagList.Contains(ECliFlag.Recursive);
        private List<ECliFlag> CliFlagList { get; }
        public CliArguments(string[] args)
        {
            if (args.Length < 3)
                throw new Exception("Too less arguments to run! Please have a look at the readme in the repository!");
            JobName = FilterName(args[0]);
            Source = new Target(args[1]);
            Destination = new Target(args[2]);
            CliFlagList = CliFlags.ParseFlags(args);
        }

        ///
        // https://unix.stackexchange.com/a/23602
        ///
        private string FilterName(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (c == ' ' || c == '-' || c == '.' || c == ':' || (c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122)) // 0-9 / a-z / A-Z
                {
                    sb.Append(c);
                }
                else if (c == '_')
                    sb.Append('-');
            }
            string resultStr = sb.ToString();
            if (string.IsNullOrWhiteSpace(resultStr))
                resultStr = "unknown";
            if (!resultStr.Equals(input))
                Console.WriteLine($"*** Warning * The Job name was changed from '{input}' to '{resultStr}' because it was invalid.");
            return sb.ToString();
        }
    }
}