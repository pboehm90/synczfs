using System;
using System.Threading;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.Syncer;
using synczfs.ZFS;

namespace synczfs
{
    class Program
    {
        static void Main(string[] args)
        {
            CliArguments myArgs = new CliArguments(args);
            SyncerProcess.CreateInstance(myArgs).Run();
        }
    }
}
