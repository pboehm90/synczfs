using System.Collections.Generic;

namespace synczfs.CLI
{
    public class CliFactory
    {
        public Dictionary<string, string> KeyValue { get; }

        public CliFactory(string[] args)
        {
            KeyValue = new Dictionary<string, string>();

            string lastCommand = null;
            foreach (string arg in args)
            {
                string key = arg.ToLowerInvariant().Trim();
                if (key.StartsWith('-'))
                {
                    if (lastCommand != null)
                        KeyValue[lastCommand] = null;
                    lastCommand = key;
                }
                else if (lastCommand != null)
                {
                    KeyValue[lastCommand] = arg;
                    lastCommand = null;
                }
            }
            if (lastCommand != null)
                KeyValue[lastCommand] = null;
        }
    }
}