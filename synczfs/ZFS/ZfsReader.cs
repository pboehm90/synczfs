using System.Collections.Generic;
using System.Text;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.ZFS.Objects;

namespace synczfs.ZFS
{
    class ZfsReader
    {
        public static ZfsReader GetByPath(Target target, bool recursive)
        {
            return new ZfsReader(target, recursive);
        }

        Target Target { get; }
        public string Path { get; }
        public bool Recursive { get; }
        public List<Dataset> Datasets { get; private set; }
        
        private ZfsReader(Target target, bool recursive) : this(target, target.ZfsPath, recursive)
        {   }

        public ZfsReader(Target target, string zfsPath, bool recursive)
        {
            Target = target;
            Path = zfsPath;
            Recursive = recursive;
        }

        public void Read()
        {
            List<ZfsListRow> list = List();
            Datasets = ReadDatasets(list);

            Logging.GetInstance().Log($"Dataset informationen aus dem Target {Target.ZfsPath} erfolgreich gelesen!");
        }

        public Dataset GetUpdatedDataset(Dataset oldDs)
        {
            Read();
            HierachicalDatasetToList hdstl = new HierachicalDatasetToList(Datasets);
            return hdstl.GetByDataset(oldDs);
        }

        private List<ZfsListRow> List()
        {
            string command = "zfs list -p -t all -r " + Path;
            
            //var proc = ShellProcess.RunNew(Target, command).WaitForExit();
            var proc = SimpleShellProcess.Run(Target, command);

            if (proc.StandardOutputLines.Length == 1)
                throw new System.Exception("The path could not found!");
            
            List<ZfsListRow> zfsPaths = new List<ZfsListRow>();

            for (int i = 1; i < proc.StandardOutputLines.Length; i++)
            {
                string line = proc.StandardOutputLines[i];
                new ZfsListRow(line);
                string zfsName = line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)[0];

                if (Recursive)
                    zfsPaths.Add(new ZfsListRow(line));
                else
                {
                    if (string.Compare(Path, zfsName, true) == 0)
                        zfsPaths.Add(new ZfsListRow(line));
                    else if (zfsName.StartsWith(Path + "@"))
                        zfsPaths.Add(new ZfsListRow(line));;
                }
            }

            // Sortierung der Zeilen zum Sicherstellen der gültigkeit???

            return zfsPaths;
        }

        private List<Dataset> ReadDatasets(List<ZfsListRow> lines)
        {
            List<Dataset> datasets = new List<Dataset>();

            Dataset current = null;

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsPool)
                    continue; // Is nur Poolname, kein Dataset

                string[] split = lines[i].Name.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

                
                if (lines[i].IsSnapshot)
                {
                    current.AddSnapshot(lines[i]);
                }
                else
                {
                    if (current != null)
                        datasets.Add(current);
                    current = new Dataset(lines[i].Name);
                }

                // Letzer Loop
                if (i + 1 == lines.Count)
                    datasets.Add(current);
            }
            return OrderByHierachy(datasets);
        }

        private List<Dataset> OrderByHierachy(List<Dataset> datasets)
        {
            // Ignorierliste um Childs in der iteration zu übergehen da diese ja schon zugeordnet sind
            List<Dataset> ignoreList = new List<Dataset>();
            List<Dataset> resultList = new List<Dataset>();

            foreach (Dataset dataset in datasets)
            {
                if (ignoreList.Contains(dataset))
                    continue;
                ModChildDatasets(dataset, datasets, ignoreList);
                resultList.Add(dataset);
            }

            return resultList;
        }

        private void ModChildDatasets(Dataset dataset, List<Dataset> datasets, List<Dataset> ignoreList)
        {
            foreach (Dataset ds in datasets)
            {
                if (ds.Path.StartsWith(dataset.Path))
                {
                    if (dataset.PathDeph + 1 == ds.PathDeph)
                    {
                        dataset.AddChild(ds);
                        ignoreList.Add(ds);        
                    } 
                }
            }
        }
    }
}