using System;
using System.Text;
using synczfs.CommonObjects;
using synczfs.processhelper;
using synczfs.ZFS.Objects;

namespace synczfs.ZFS
{
    class ZfsSend
    {
        CliArguments CliArguments { get; }
        Target Source { get; }
        Target Destination { get; }
        public ZfsSend(CliArguments cliArguments)
        {
            CliArguments = cliArguments;

            Source = CliArguments.Source;
            Destination = CliArguments.Destination;
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
            string cmd = AddLimitString();
            return GetSendCommand(parentSnap, childSnap) + " | " + AddLimitString() + GetReceiveCommand(zfsDestination);
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

        private string AddLimitString()
        {
            string command = "pv";
            if (!string.IsNullOrWhiteSpace(CliArguments.RateLimitSource) && CommandExists(command))
            {
                return command + " -L " + CliArguments.RateLimitSource + " | ";
            }
            return string.Empty;
        }

        private bool CommandExists(string command)
        {
            try
            {
                SimpleShellProcess.Run(Source, $"command -v {command}");
                return true;
            }
            catch
            {
                Console.WriteLine("The command '" + command + "' could not found!!!");
                return false;
            }
        }
    }
}