using System.Collections.Generic;
using synczfs.ZFS.Objects;

namespace synczfs.ZFS
{
    class HierachicalDatasetToList
    {
        public List<Dataset> AllDatasets { get; }
        public HierachicalDatasetToList(List<Dataset> datasets)
        {
            AllDatasets = new List<Dataset>();
            Iterate(datasets);
        }

        private void Iterate(List<Dataset> datasets)
        {
            if (datasets == null)
                return;
            foreach (Dataset ds in datasets)
            {
                AllDatasets.Add(ds);
                Iterate(ds.Childs);
            }
        }

        public Dataset GetByDataset(Dataset ds)
        {
            return GetByPath(ds.Path);
        }

        public Dataset GetByPath(string path)
        {
            foreach (var item in AllDatasets)
            {
                if (string.Compare(item.Path, path, true) == 0)
                    return item;
            }
            throw new System.Exception("Not found!");
        }
    }
}