using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = System.IO.File;

namespace MSOE.MediaComplete.Lib
{
    public class Importer
    {
        public delegate void ImportHandler(ImportResults results);
        public static event ImportHandler ImportFinished = delegate {};

        public async Task<ImportResults> ImportDirectory(string directory, bool isCopying)
        {
            var files = await Task.Run(() => Directory.GetFiles(directory, "*.mp3",
            SearchOption.AllDirectories));
            return await ImportFiles(files, isCopying);
        }

        public async Task<ImportResults> ImportFiles(string[] files, bool isCopying)
        {
            var homeDir = SettingWrapper.GetHomeDir();
            var results = new ImportResults
            {
                FailCount = 0,
                NewFiles = new List<FileInfo>(files.Length),
                HomeDir = new DirectoryInfo(homeDir)
            };
            
            foreach (var file in files)
            {
                var myFile = file;
                var newFile = homeDir + Path.DirectorySeparatorChar + Path.GetFileName(file);
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
                    catch (IOException exception)
                    {
                        Console.WriteLine(exception); // TODO log (MC-125)
                        results.FailCount++;
                    }
                    results.NewFiles.Add(new FileInfo(newFile));
                }
            }
            ImportFinished(results);
            return results;
        }
    }

    public class ImportResults
    {
        public List<FileInfo> NewFiles { get; set; }
        public DirectoryInfo HomeDir { get; set; }
        public int FailCount { get; set; }
    }
}
