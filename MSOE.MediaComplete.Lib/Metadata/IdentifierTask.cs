using System.Collections.Generic;
using MSOE.MediaComplete.Lib.Background;
using Sys = System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib.Metadata
{
    public class IdentifierTask : Task
    {
        public override void Do(int i)
        {
            throw new System.NotImplementedException();
        }

        public override IReadOnlyCollection<System.Type> InvalidBeforeTypes
        {
            get { throw new System.NotImplementedException(); }
        }

        public override IReadOnlyCollection<System.Type> InvalidAfterTypes
        {
            get { throw new System.NotImplementedException(); }
        }

        public override IReadOnlyCollection<System.Type> InvalidDuringTypes
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool RemoveOther(Task t)
        {
            throw new System.NotImplementedException();
        }
    }
}
