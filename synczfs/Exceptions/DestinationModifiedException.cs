using System;
using synczfs.ZFS.Objects;

namespace synczfs.Exceptions
{
    public class DestinationModifiedException : Exception, IHasExitCode
    {
        const string message = "Files on the destination Dataset were changed! The syncing Process is cancelling now! To overwrite and continue the sync use the '-force' flag! Regarding Snapshot: ";
        public DestinationModifiedException(Snapshot snapshot) : base(message + snapshot.SnapshotPath) 
        {
            
        }

        public int GetExitCode()
        {
            return 13;
        }
    }
}