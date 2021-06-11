using System;
using System.Threading;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.Syncer;
using synczfs.ZFS;
using System.Runtime.ExceptionServices;

namespace synczfs
{
    class Program
    {
        static int Main(string[] args)
        {
            Exception fatalException = null;
            CliArguments myArgs = new CliArguments(args);
            try
            {
                SyncerProcess.CreateInstance(myArgs).Run();    
            }
            catch (Exception ex)
            {
                Globals.LogInstance?.Log(ex);
                fatalException = ex;
            }
            finally
            {
                // Cleanup
                myArgs.Source.Shell.Dispose();
                myArgs.Destination.Shell.Dispose();
                Globals.LogInstance?.Dispose();
            }

            if (fatalException != null)
                ExceptionDispatchInfo.Capture(fatalException).Throw();;

            return Environment.ExitCode;
        }
    }
}
