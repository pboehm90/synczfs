using System;
using System.Collections.Generic;
using System.Text;
using synczfs.CLI;

namespace synczfs.CommonObjects
{
    public class CliArguments
    {
        public string JobName { get; }
        public Target Source { get; }
        public Target Destination { get; }
        public bool Recursive => CliFlagList.Contains(ECliFlag.Recursive);
        public bool SendProps => CliFlagList.Contains(ECliFlag.SendProps);
        public string RateLimitSource { get; }
        private List<ECliFlag> CliFlagList { get; }
        // Ignores all non-zsyncd snapshots e.g. manual created ones
        public bool AutoSnapOnly => CliFlagList.Contains(ECliFlag.AutoSnapOnly);
        public CliArguments(string[] args)
        {
            if (args.Length < 3)
                throw new Exception("Too less arguments to run! Please have a look at the readme in the repository!");
            JobName = FilterName(args[0].Trim().ToLowerInvariant());
            Source = new Target(args[1]);
            Destination = new Target(args[2]);
            CliFlagList = CliFlags.ParseFlags(args);

            CliFactory fac = new CliFactory(args);
            var dict = fac.KeyValue;

            RateLimitSource = GetRateLimit(fac);
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

        private string GetRateLimit(CliFactory factory)
        {
            string key = "-sourcelimit";

            try
            {
                if (factory.KeyValue.ContainsKey(key))
                {
                    string limitValue = factory.KeyValue[key].ToUpperInvariant().Trim();
                    if (!string.IsNullOrWhiteSpace(limitValue))
                    {
                        string strNum = limitValue.Substring(0, limitValue.Length - 1);
                        char sizeUnit = limitValue[limitValue.Length - 1];

                        if (sizeUnit == 'K' || sizeUnit == 'M' || sizeUnit == 'G' || sizeUnit == 'T')
                        {
                            int.Parse(strNum);
                            return limitValue;
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Invalid source Limit argument!");
            }

            return null;
        }
    }
}