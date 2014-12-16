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
        public static event ImportHandler ImportFinished = delegate {};

        private readonly string _homeDir;

        public Importer(string dir)
        {
            _homeDir = dir;
        }


        public async Task ImportDirectory(string directory, bool isCopy)
        {
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            await Task.Run(() => ImportFiles(files, isCopy));
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
        }

        public async Task ImportFiles(string[] files, bool isCopy)
        {

            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Started", StatusBarHandler.StatusIcon.Working);
            var newFiles = new List<FileInfo>(files.Length);
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = _homeDir + Path.DirectorySeparatorChar + Path.GetFileName(file);
                if (File.Exists(newFile)) continue;
                try
                {
                    if (isCopy)
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
                    StatusBarHandler.Instance.ChangeStatusBarMessage("Importing-Error",
                        StatusBarHandler.StatusIcon.Error);
                }
                newFiles.Add(new FileInfo(newFile));
            }
            ImportFinished(newFiles, new DirectoryInfo(_homeDir));
            StatusBarHandler.Instance.ChangeStatusBarMessage("Import-Success", StatusBarHandler.StatusIcon.Success);
        }
    }
}
