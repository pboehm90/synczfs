using synczfs.processhelper;

namespace synczfs.CommonObjects
{
    public class Target
    {
        string TargetString { get; }
        public bool UseSsh { get; private set; }
        public string Username { get; private set; }
        public string Host { get; private set; }
        public string ZfsPath { get; private set; }
        public ushort SshPort {get; private set;} = 22;
        public SimpleProcessBase Shell { get; private set; }
        public Target(string target)
        {
            TargetString = target;
            if (!ParseSsh())
                ZfsPath = target;

            ObtainShell();
        }

        private void ObtainShell()
        {
            if (UseSsh)
                Shell = new SimpleSshConnection(this);
            else
                Shell = new SimpleShellProcess(this);
        }

        private bool ParseSsh()
        {
            string[] splitUser = TargetString.Split('@');
            if (splitUser.Length == 2)
            {
                Username = splitUser[0];
                string[] splitHost = splitUser[1].Split("://", System.StringSplitOptions.None);
                if (splitHost.Length == 2)
                {
                    ParseHost(splitHost[0]);
                    ZfsPath = splitHost[1];
                    UseSsh = true;
                    return true;
                }
            }
            return false;
        }

        private void ParseHost(string hostString)
        {
            string[] split = hostString.Split(':');
            Host = split[0];
            if (split.Length == 2)
                SshPort = ushort.Parse(split[1]);
        }
    }
}