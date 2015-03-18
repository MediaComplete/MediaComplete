using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class FileMover
    {
        public static void MoveDirectory(string source, string dest)
        {
            if(Directory.Exists(dest)) throw new IOException("Destination directory already exists");
            var sourceDir = new DirectoryInfo(source);

            var folders = sourceDir.GetDirectories().ToList();
            var files = sourceDir.GetFiles().ToList();

            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);

            folders.ForEach(x => x.MoveTo(dest +Path.DirectorySeparatorChar +x.Name));
            files.ForEach(x => x.MoveTo(dest +Path.DirectorySeparatorChar + x.Name));

            if(sourceDir.GetDirectories().Length == 0 && sourceDir.GetFiles().Length == 0) sourceDir.Delete();

        }
    }
}
