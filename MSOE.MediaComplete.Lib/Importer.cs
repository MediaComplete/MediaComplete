using System;
using System.IO;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        private readonly string _homeDir;

        public Importer(string dir)
        {
            _homeDir = dir;
        }


        public async void ImportDirectory(string directory)
        {
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            await Task.Run(() => ImportFiles(files));
        }

        public async void ImportFiles(string[] files)
        {
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = _homeDir + Path.GetFileName(file);
                if (!File.Exists(newFile))
                {
                    try
                    {
                        await Task.Run(() => File.Copy(myFile, newFile));
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }

        }
    }
}
