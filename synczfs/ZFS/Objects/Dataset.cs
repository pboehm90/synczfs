using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace synczfs.ZFS.Objects
{
    public class Dataset
    {
        public string Path { get; }
        public string PathWithoutPool { get; }
        public string PoolName { get; }
        List<Snapshot> Snapshots { get; }
        public List<Dataset> Childs { get; }
        public int PathDeph => PathWithoutPool.Split('/').Length;
        public Dataset(string path)
        {
            Path = path;

            StringBuilder sbPool = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            bool slashCame = false;
            foreach (char c in Path)
            {
                if (slashCame)
                    sb.Append(c);
                else if (c == '/')
                    slashCame = true;
                else
                    sbPool.Append(c);
            }
            PathWithoutPool = sb.ToString();
            PoolName = sbPool.ToString();

            Snapshots = new List<Snapshot>();
            Childs = new List<Dataset>();
        }

        public void AddChild(Dataset dataset)
        {
            Childs.Add(dataset);
        }
        
        public void AddSnapshot(ZfsListRow zfsListRow)
        {
            Snapshots.Add(new Snapshot(zfsListRow, this));
        }

        public List<Snapshot> GetSnapshotsScoped(string jobName)
        {
            return Snapshots.Where(x => !x.IsZsyncdSnapshot || x.JobName.Equals(jobName)).ToList();
        }

        public override string ToString()
        {
            return Path;
        }
    }
}