using System.Collections.Generic;

namespace synczfs.CommonObjects
{
    class CliFlags
    {
        static Dictionary<string, ECliFlag> FlagsDict = new Dictionary<string, ECliFlag> 
            { 
                ["-r"] = ECliFlag.Recursive,
                ["-onlychilds"] = ECliFlag.ChildsOnly,
                ["-autosnaponly"] = ECliFlag.AutoSnapOnly
            };
        
        public static List<ECliFlag> ParseFlags(string[] args)
        {
            List<ECliFlag> resultFlags = new List<ECliFlag>();
            foreach (string arg in args)
            {
                string argLower = arg.ToLowerInvariant().Trim();
                if (FlagsDict.ContainsKey(argLower))
                    resultFlags.Add(FlagsDict[argLower]);
            }
            return resultFlags;
        }
    }
}