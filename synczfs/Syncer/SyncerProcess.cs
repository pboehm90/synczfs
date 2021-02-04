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
            snapshotUnique = Guid.NewGuid().ToString("N").ToLowerInvariant().Substring(0, 6);

            Cli = cliArguments;
            ReaderSource = ZfsReader.GetByPath(Cli.Source, Cli.Recursive);
        }

        public void Run()
        {
            // Erste Ebende der Datasets Snapshotten und syncen --> Danach iterativ
            string mutexHash = ToolBox.HashStringSha256(("synczfs_" + Cli.JobName).Trim().ToLowerInvariant());
            string globalMutexString = @"Global\" + mutexHash;
            try
            {
                using (Mutex mtx = new Mutex(false, globalMutexString))
                {
                    if (mtx.WaitOne(1000))
                    {
                        Logging.GetInstance().Log("Mutex erhalten!");
                        try
                        {
                            DoSync();
                        }
                        finally
                        {
                            mtx.ReleaseMutex();
                            Logging.GetInstance().Log("Mutex released!");
                        }
                    }
                    else
                        Console.WriteLine($"Job '{Cli.JobName}' is already running. Cancelling...");
                }
            }
            catch (Exception ex)
            {
                FlushException(ex);
                throw;
            }
            finally
            {
                Logging.GetInstance().Dispose();
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
                FlushException(modifiedEx);
            }
        }

        private void FlushException(Exception ex)
        {
            int exitCode = 99; // Generic fatal failure!

            Logging.GetInstance().Log(ex.ToString());
            Console.Error.WriteLine(ex.ToString());
            if (ex is IHasExitCode)
                exitCode = ((IHasExitCode)ex).GetExitCode();
            System.Environment.ExitCode = exitCode;
            Logging.GetInstance().FlushToFile(Cli.JobName);
        }
    }
}