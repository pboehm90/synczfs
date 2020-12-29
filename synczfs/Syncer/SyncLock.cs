using System.Threading;

namespace synczfs.Syncer
{
    class SyncLock
    {
        Mutex LockMutex { get; set; }
        string JobName {get;}
        public SyncLock(string jobName)
        {
            JobName = jobName;
        }

        public bool GetLock()
        {
            LockMutex = new Mutex(false, JobName);
            try
            {
                return LockMutex.WaitOne(5000);
            }
            catch (AbandonedMutexException ex)
            {
                if (ex.Mutex != null)
                {
                    ex.Mutex.ReleaseMutex();
                    ex.Mutex.Dispose();
                }
            }
            // Deadlock???
            return GetLock();
        }

        public void Release()
        {
            if (LockMutex != null)
            {
                LockMutex.ReleaseMutex();
                LockMutex.Dispose();
            }
        }
    }
}