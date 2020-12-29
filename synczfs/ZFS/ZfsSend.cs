using System.Text;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.ZFS.Objects;

namespace synczfs.ZFS
{
    class ZfsSend
    {
        Target Source { get; }
        Target Destination { get; }
        public ZfsSend(Target source, Target destination)
        {
            Source = source;
            Destination = destination;
        }

        public void Send(Snapshot snapshot, string zfsDestination)
        {
            SimpleShellProcess.Run(Source, GetFullCommand(snapshot.SnapshotPath, null, zfsDestination));
            Mount(zfsDestination);
        }

        public void SendIncremental(string parentSnap, string childSnap, string zfsDestination)
        {
            SimpleShellProcess.Run(Source, GetFullCommand(parentSnap, childSnap, zfsDestination));
            Mount(zfsDestination);
        }

        private string GetFullCommand(string parentSnap, string childSnap, string zfsDestination)
        {
            return GetSendCommand(parentSnap, childSnap) + " | " + GetReceiveCommand(zfsDestination);
        }

        private void Mount(string zfsDestination)
        {
            try
            {
                var df = SimpleShellProcess.Run(Destination, "df | grep " + zfsDestination).StandardOutput;
            }
            catch (ProcessException pex)
            {
                if (pex.ExitCode == 1)
                {
                    try 
                    {
                        SimpleShellProcess.Run(Destination, "zfs mount " + zfsDestination);
                    }
                    catch (ProcessException) { /* Fire and forget */ }
                }
                else
                    throw;
            }
        }

        private string GetSendCommand(string parentSnap, string childSnap)
        {
            StringBuilder sb = new StringBuilder("zfs send ");
            
            if (childSnap != null)
                sb.Append(" -i ");

            sb.Append(parentSnap);

            if (childSnap != null)
                sb.Append(" " + childSnap);
            
            return sb.ToString();
        }

        private string GetReceiveCommand(string zfsTargetPath)
        {
            StringBuilder sb = new StringBuilder();
            if (Destination.UseSsh)
                sb.Append("ssh " + Destination.Host + " ");
            sb.Append("zfs recv -F " + zfsTargetPath);
            return sb.ToString();
        }
    }
}