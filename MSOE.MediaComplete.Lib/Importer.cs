using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Properties;
using File = System.IO.File;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public delegate void ImportHandler(List<FileInfo> files, DirectoryInfo homeDir);
        public static event ImportHandler ImportFinished = delegate {};

        public string HomeDir { get; set; }
        private static Importer _instance;
        private Importer()
        {
            SettingWrapper.RaiseSettingEvent += HandleSettingChangeEvent;
        }

        public static Importer Instance
        {
            get { return _instance ?? (_instance = new Importer()); }
        }

        private static void HandleSettingChangeEvent()
        {
            Instance.HomeDir = SettingWrapper.GetHomeDir();
        }

        public async Task ImportDirectory(string directory, bool isCopying)
        {
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            await ImportFiles(files, isCopying);
        }

        public async Task ImportFiles(string[] files, bool isCopying)
        {
            var newFiles = new List<FileInfo>(files.Length);
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = HomeDir + Path.DirectorySeparatorChar + Path.GetFileName(file);
                if (!File.Exists(newFile))
                {
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
                    newFiles.Add(new FileInfo(newFile));
                }
            }
            ImportFinished(newFiles, new DirectoryInfo(HomeDir));
        }
    }
}
