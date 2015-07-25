using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Search
{
    public interface IIndex
    {
        string IndexLocation { get; set; }
        Results Search(Query query);
        void UpdateEntries(params Entry[] entries);
    }
}
