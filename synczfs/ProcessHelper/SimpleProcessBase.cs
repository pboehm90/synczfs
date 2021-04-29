using System;
using synczfs.CommonObjects;

namespace synczfs.processhelper
{
    public abstract class SimpleProcessBase : IDisposable
    {
        internal Target CurrentTarget { get; }
        object RunLock { get; } = new object();

        public SimpleProcessBase(Target target)
        {
            CurrentTarget = target;
        }

        public ProcessResult Run(string command)
        {
            lock (RunLock)
            {
                Logging.GetInstance().Log("Start process! " + command);
                ProcessResult result = ExecInternal(command);
                if (!result.OK)
                    throw new ProcessException(result.ReturnCode, command, result.StdErr); 
                Logging.GetInstance().Log("End process! " + command);
                return result;
            }
        }

        internal abstract ProcessResult ExecInternal(string command);

        public virtual void Dispose()
        {   }
    }
}