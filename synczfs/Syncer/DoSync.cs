using System;
using System.Collections.Generic;
using synczfs.CommonObjects;
using synczfs.Exceptions;
using synczfs.processhelper;
using synczfs.ZFS;
using synczfs.ZFS.Objects;

namespace synczfs.Syncer
{
    class DoSync
    {
        CliArguments CliArguments { get; }
        Dataset SourceDataset { get; set; }
        ZfsReader ReaderSource { get; }
        ZfsReader ReaderDestination { get; }
        string SnapshotUnique { get; }
        public DoSync(string snapshotUnique, CliArguments cliArguments, Dataset sourceDataset)
        {
            SnapshotUnique = snapshotUnique;
            CliArguments = cliArguments;
            SourceDataset = sourceDataset;

            ReaderSource = new ZfsReader(cliArguments.Source, sourceDataset.Path, true);
            ReaderSource.Read();

            ReaderDestination = new ZfsReader(cliArguments.Destination, GetDestinationPath(cliArguments, sourceDataset), true);
        }

        private string GetDestinationPath(CliArguments cliArguments, Dataset sourceDataset)
        {
            string diff = sourceDataset.Path.Substring(cliArguments.Source.ZfsPath.Length, sourceDataset.Path.Length - cliArguments.Source.ZfsPath.Length);
            string result = cliArguments.Destination.ZfsPath + diff;
            return result;
        }

        public void Start()
        {
            Snapshot createdSnapshot = null;
            SourceDataset = CreateSnapshot(CliArguments.Source, out createdSnapshot);
            try
            {
                ExecSync(SourceDataset);
            }
            catch (Exception)
            {
                DestroySnapshot(CliArguments.Source, createdSnapshot);
                throw;
            }
        }

        private void ExecSync(Dataset sourceDataset)
        {
            string destination = GetDestinationPath(CliArguments, sourceDataset);
            Dataset destinationDataset = GetDestinationDatasetByPath(destination);
            bool isInitialSync = destinationDataset == null;

            int sourceSnapBeginIndex;
            if (isInitialSync)
            {
                // Initial Sync
                SendOldestSnapshotInitial(CliArguments.Source, sourceDataset, destination);
                destinationDataset = GetDestinationDatasetByPath(destination);

                sourceSnapBeginIndex = GetCommonSnapshot(sourceDataset, destinationDataset, CliArguments.AutoSnapOnly);
            }
            else
            {
                sourceSnapBeginIndex = GetCommonSnapshot(sourceDataset, destinationDataset, true);
            }
            // Nur die Snapshots dieses Jobs oder manuell erstellte interessieren!
            List<Snapshot> scopedList = sourceDataset.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly);

            Snapshot lastSyncedZsyncSnap = null;
            for (int i = sourceSnapBeginIndex; i < scopedList.Count; i++)
            {
                Snapshot parentSnapshot = scopedList[i];
                if (i + 1 < scopedList.Count)
                {
                    Snapshot childSnapshot = scopedList[i + 1];
                    SendIncremental(parentSnapshot.SnapshotPath, childSnapshot.SnapshotPath, destination);
                    if (childSnapshot.IsZsyncdSnapshot)
                        lastSyncedZsyncSnap = childSnapshot;
                }
            }
            
            destinationDataset = GetDestinationDatasetByPath(destination);

            if (lastSyncedZsyncSnap != null)
            {
                CleanUp(CliArguments.Source, lastSyncedZsyncSnap, sourceDataset);
                CleanUp(CliArguments.Destination, lastSyncedZsyncSnap, destinationDataset);
            }
        }

        private Dataset GetDestinationDatasetByPath(string path)
        {
            try
            {
                ReaderDestination.Read();

                HierachicalDatasetToList hdstl = new HierachicalDatasetToList(ReaderDestination.Datasets);
                foreach (Dataset ds in hdstl.AllDatasets)
                {
                    if (ds.Path.Equals(path))
                        return ds;
                }
            }
            catch (ProcessException pex)
            {
                // Tritt auf wenn das Ziel Dataset noch nicht existiert. Wird ja beim sync angelegt!
                Logging.GetInstance().Log("Dataset Informationen des Ziels konnten nicht gelesen werden. Zieldataset müsste nicht existieren." + Environment.NewLine + pex.ToString());
            }
            
            return null;
        }

        private void SendIncremental(string parentSnap, string childSnap, string destination)
        {
            ZfsSend send = new ZfsSend(CliArguments);
            send.SendIncremental(parentSnap, childSnap, destination);
        }

        private void SendOldestSnapshotInitial(Target target, Dataset sourceDataset, string destination)
        {
            ZfsSend send = new ZfsSend(CliArguments);

            Snapshot firstSnap = sourceDataset.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly)[0];
            send.Send(firstSnap, destination);
        }

        private int GetCommonSnapshot(Dataset sourceDataset, Dataset destinationDataset, bool getNewest)
        {
            bool doBreak = false;
            int commonSnapIndex = -1;

            List<Snapshot> sourceList = sourceDataset.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly);
            List<Snapshot> destinationList = destinationDataset.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly);

            Snapshot lastCommonDestinationSnap = null;
            for (int i = destinationList.Count - 1; i >= 0; i--)
            {
                if (doBreak)
                    break;
                for (int j = 0; j < sourceList.Count; j++)
                {
                    if (sourceList[j].Name.Equals(destinationList[i].Name))
                    {
                        commonSnapIndex = j;
                        lastCommonDestinationSnap = destinationList[i];
                        if (getNewest)
                        {
                            doBreak = true;
                            break;
                        }
                    }
                }
            }

            if (commonSnapIndex == -1)
                throw new NotImplementedException("No Common snapshot found! Destination has to be purged!");

            //CheckForModification(lastCommonDestinationSnap);
            
            return commonSnapIndex;
        }

        // For secure syncing. Checks if data on the source Dataset was modified since the last sync. Otherwie the changes will be overwritten!
        private void CheckForModification(Snapshot lastCommonDestinationSnap)
        {
            if (lastCommonDestinationSnap.Used > 0)
            {
                throw new DestinationModifiedException(lastCommonDestinationSnap);
            }
        }

        private void CleanUp(Target target, Snapshot lastSyncedZdyncdSnap, Dataset dsToClean)
        {
            List<Snapshot> sourceList = dsToClean.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly);

            bool canClean = false;
            for (int i = sourceList.Count - 1; i >= 0; i--)
            {
                if (canClean && sourceList[i].IsZsyncdSnapshot)
                {
                    SimpleShellProcess.Run(target, "zfs destroy " + sourceList[i].SnapshotPath);
                }
                else if (!canClean && sourceList[i].Name.Equals(lastSyncedZdyncdSnap.Name))
                    canClean = true;
            }
        }

        private Dataset CreateSnapshot(Target target, out Snapshot createdSnapshot)
        {
            string snapName = $"synczfs_{CliArguments.JobName}_{DateTime.Now.ToString("dd_MM_yyyy__HH_mm_ss")}__{SnapshotUnique}";

            string command = "zfs snapshot " + $"{SourceDataset.Path}@{snapName}";

            SimpleShellProcess.Run(target, command);

            Dataset updatedDataset = ReaderSource.GetUpdatedDataset(SourceDataset);

            foreach (Snapshot snap in updatedDataset.GetSnapshotsScoped(CliArguments.JobName, CliArguments.AutoSnapOnly))
            {
                if (snap.Name.Equals(snapName))
                {
                    createdSnapshot = snap;
                    return updatedDataset;
                }
            }
            throw new NotImplementedException();
        }

        private void DestroySnapshot(Target target, Snapshot snapshot)
        {
            string command = "zfs destroy " + snapshot.SnapshotPath;
            SimpleShellProcess.Run(target, command);
        }
    }
}