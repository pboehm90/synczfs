using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using synczfs.CommonObjects;
using synczfs.Exceptions;
using synczfs.processhelper;
using synczfs.ZFS;
using synczfs.ZFS.Objects;

namespace synczfs.Syncer
{
    public class SyncerProcess
    {
        string snapshotUnique;
        CliArguments Cli { get; }
        ZfsReader ReaderSource { get; }

        public static SyncerProcess CreateInstance(CliArguments cliArguments)
        {
            return new SyncerProcess(cliArguments);
        }

        private SyncerProcess(CliArguments cliArguments)
        {
            Globals.InitLog(cliArguments.JobName);
            snapshotUnique = Guid.NewGuid().ToString("N").ToLowerInvariant().Substring(0, 6);

            Cli = cliArguments;
            ReaderSource = ZfsReader.GetByPath(Cli.Source, Cli.Recursive);
        }

        public void Run()
        {
            // Erste Ebende der Datasets Snapshotten und syncen --> Danach iterativ
            string mutexHash = ToolBox.HashStringSha256(("synczfs_" + Cli.JobName).Trim().ToLowerInvariant());
            string globalMutexString = "Global\\" + mutexHash;
            
            using (Mutex mtx = new Mutex(false, globalMutexString))
            {
                if (mtx.WaitOne(1000))
                {
                    Globals.LogInstance.Log("Got Mutex!");
                    try
                    {
                        DoSync();
                    }
                    finally
                    {
                        mtx.ReleaseMutex();
                        Globals.LogInstance.Log("Mutex released!");
                    }
                }
                else
                    Console.WriteLine($"Job '{Cli.JobName}' is already running. Cancelling...");
            }
        }

        private void DoSync()
        {
            ReaderSource.Read();
            foreach (Dataset rootDs in ReaderSource.Datasets)
            {
                DoSync(rootDs);
                if (Cli.Recursive)
                    Iterate(rootDs.Childs);
            }
        }

        private void Iterate(List<Dataset> datasets)
        {
            foreach (var item in datasets)
            {
                DoSync(item);
                Iterate(item.Childs);
            }
        }

        private void DoSync(Dataset sourceDataset)
        {
            try
            {
                DoSync syncccrr = new DoSync(snapshotUnique, Cli, sourceDataset);
                syncccrr.Start();
            }
            catch (DestinationModifiedException modifiedEx)
            {
                Globals.LogInstance.Log(modifiedEx);
            }
        }
    }
}