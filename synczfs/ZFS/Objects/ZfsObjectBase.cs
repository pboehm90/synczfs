using System.Collections.Generic;

namespace synczfs.ZFS.Objects
{
    class ZfsObjectBase
    {
        public List<Dataset> Childs { get; }

        public ZfsObjectBase()
        {
            
        }
    }
}