using System;
using System.Collections.Generic;

namespace synczfs.ZFS.Objects
{
    public class ZfsListRow
    {
        public string Name { get; }
        public ulong USED { get; }
        public ulong AVAIL { get; }
        public ulong REFER { get; }
        public string MOUNTPOINT { get; }
        public bool IsPool => !Name.Contains('/');
        public bool IsSnapshot => !IsPool && Name.Contains('@');
        public ZfsListRow(string row)
        {
            string[] split = GetSplit(row);
            if (split.Length != 5)
                throw new NotImplementedException();
            
            Name = split[0];
            USED = UInt64.Parse(split[1]);
            if (!IsSnapshot)
                AVAIL = UInt64.Parse(split[2]);
            REFER = UInt64.Parse(split[3]);
            MOUNTPOINT = split[4];
        }

        private string[] GetSplit(string row)
        {
            List<string> result = new List<string>();
            foreach (string split in row.Split(' ', System.StringSplitOptions.RemoveEmptyEntries))
            {
                result.Add(split.Trim());
            }
            return result.ToArray();
        }
    }
}