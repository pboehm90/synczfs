using System;

namespace synczfs.ZFS.Objects
{
    public class Snapshot
    {
        public Dataset Dataset { get; }
        public string Name { get; set; }
        public string SnapshotPath => Dataset.Path + "@" + Name;
        public bool IsZsyncdSnapshot => Name.StartsWith("synczfs_");
        public string JobName { get; }
        public ulong Used { get; } // Space used by this Snapshot in Bytes
        public Snapshot(ZfsListRow zfsListRow, Dataset dataset)
        {
            Dataset = dataset;
            Used = zfsListRow.USED;

            string[] split = zfsListRow.Name.Split('@');
            if (split.Length == 1)
                Name = split[0];
            else if (split.Length == 2)
                Name = split[1];
            else
                throw new NotImplementedException();

            if (IsZsyncdSnapshot)
            {
                string[] splitUnderline = Name.Split('_');
                JobName = splitUnderline[1];
            }
        }
    }
}