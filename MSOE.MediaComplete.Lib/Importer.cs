using System;
using System.IO;
using System.Threading.Tasks;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        private static Importer _instance;
        private Importer() { }

        public static Importer Instance
        {
            get { return _instance ?? (_instance = new Importer()); }
        }

        public async Task ImportDirectory(string directory, bool isCopying)
        {
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            await ImportFiles(files, isCopying);
        }

        public async Task ImportFiles(string[] files, bool isCopying)
        {
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = SettingWrapper.GetHomeDir() + Path.GetFileName(file);
                if (File.Exists(newFile)) continue;
                try
                {
                    if (isCopying)
                    {
                        await Task.Run(() => File.Copy(myFile, newFile));
                    }
                    else
                    {
                        await Task.Run(() => File.Move(myFile, newFile));
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}
