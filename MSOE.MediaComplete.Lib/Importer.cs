using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public delegate void ImportHandler(List<FileInfo> files, DirectoryInfo homeDir);

        public static event ImportHandler ImportFinished = delegate { };

        public string HomeDir { get; set; }
        private static Importer _instance;

        private Importer()
        {
        }

        public static Importer Instance
        {
            get { return _instance ?? (_instance = new Importer()); }
        }

        public async Task ImportDirectory(string directory, bool isCopying)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
                SearchOption.AllDirectories));
            await ImportFiles(files, isCopying);
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
        }

        public async Task ImportFiles(string[] files, bool isCopying)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);
            var newFiles = new List<FileInfo>(files.Length);
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = HomeDir + Path.GetFileName(file);
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
                    catch (Exception)
                    {
                        StatusBarHandler.Instance.ChangeStatusBarMessage("Importing-Error",
                            StatusBarHandler.StatusIcon.Error);
                    }
                    newFiles.Add(new FileInfo(newFile));
                }
            }
            ImportFinished(newFiles, new DirectoryInfo(HomeDir));
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
        }
    }
}
