namespace synczfs.CommonObjects
{
    public class Globals
    {
        public static Logging LogInstance { get; private set; }

        public static void InitLog(string jobName)
        {
            LogInstance = new Logging(jobName);
        }
    }
}