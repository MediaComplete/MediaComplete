using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var homeDir = new DirectoryInfo(SettingWrapper.GetHomeDir());
            var files = Array.FindAll(Directory.GetFiles(directory, "*.mp3", SearchOption.AllDirectories),
                s => !new FileInfo(s).HasParent(homeDir));
            
            return await ImportFiles(files, isCopying);
        }

        public async Task<ImportResults> ImportFiles(string[] files, bool isCopying)
        {
            // First validate that we're not to trying to do a circular import (from library to library)
            var homeDir = SettingWrapper.GetHomeDir();

            if (files.Any(f => new FileInfo(f).HasParent(new DirectoryInfo(homeDir))))
            {
                throw new InvalidImportException();
            }

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
                        results.NewFiles.Add(new FileInfo(newFile));
                    }
                    catch (IOException exception)
                    {
                        Console.WriteLine(exception); // TODO log (MC-125)
                        results.FailCount++;
                    }
                }
            }
            ImportFinished(results);
            return results;
        }
    }

    /// <summary>
    /// A struct-class containing post-mortem data on an import operation.
    /// </summary>
    public class ImportResults
    {
        public List<FileInfo> NewFiles { get; set; }
        public DirectoryInfo HomeDir { get; set; }
        public int FailCount { get; set; }
    }

    /// <summary>
    /// Thrown when an import is attempted on files in the library directory. 
    /// </summary>
    public class InvalidImportException : Exception
    {
        public InvalidImportException() : base("Cannot import a file already located in the library directory tree!")
        {
            
        }
    }
}
