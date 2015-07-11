using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Files;

namespace MSOE.MediaComplete.Lib
{
    public class NinjectBindings:Ninject.Modules.NinjectModule 
    {
        public override void Load()
        {
            Bind<IFileManager>().To<FileManager>().InSingletonScope();
        }
    }
}
