using System;
using System.IO;
using System.Threading.Tasks;
using MSOE.MediaComplete;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public string _homeDir { get; set; }
        private static Importer instance;
        private Importer()
        {
        }

        public static Importer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Importer();
                }
                return instance;
            }
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
