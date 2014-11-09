using System;
using System.IO;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public string HomeDir { get; set; }
        private static Importer _instance;
        private Importer()
        {
        }

        public static Importer Instance
        {
            get { return _instance ?? (_instance = new Importer()); }
        }


        public async Task ImportDirectory(string directory)
        {
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            await Task.Run(() => ImportFiles(files));
        }

        public async Task ImportFiles(string[] files)
        {
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = HomeDir + Path.GetFileName(file);
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
