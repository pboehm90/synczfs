namespace synczfs.CommonObjects
{
    public class Target
    {
        string TargetString { get; }
        public bool UseSsh { get; private set; }
        public string Username { get; private set; }
        public string Host { get; private set; }
        public string ZfsPath { get; private set; }
        public Target(string target)
        {
            TargetString = target;
            if (!ParseSsh())
                ZfsPath = target;
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
                    Host = splitHost[0];
                    ZfsPath = splitHost[1];
                    UseSsh = true;
                    return true;
                }
            }
            return false;
        }
    }
}