using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Search.Lucene
{
    internal class LuceneIndex : IIndex
    {
        public string IndexLocation
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Results Search(Query query)
        {
            throw new NotImplementedException();
        }

        public void UpdateEntries(params Entry[] entries)
        {
            throw new NotImplementedException();
        }
    }
}
