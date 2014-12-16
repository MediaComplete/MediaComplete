using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PollingTest
    {
        private static FileInfo _file;
        private const string DirectoryPath = "C:\\TESTinboxForLibrary\\";
        private const string FileName = "file.mp3";

        /// <summary>
        /// creates the directory and file to check
        /// </summary>
        [TestInitialize]
        public  void Before()
        {
            Directory.CreateDirectory(DirectoryPath);
            _file = new FileInfo(DirectoryPath + FileName);
            File.Create(_file.FullName).Close();
        }

        /// <summary>
        /// deletes the file and directory if they still exist
        /// </summary>
        [TestCleanup]
        public void After()
        {
            if (File.Exists(_file.FullName))
            {
                File.Delete(_file.FullName);
            }
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath);
            }
        }

        /// <summary>
        /// tests that it calls the delegate
        /// </summary>
        [TestMethod]
        [Timeout(40000)]//Milliseconds
        public void OnTimerFinishedTest()
        {
            var pass = false;
            SettingWrapper.SetInboxDir(DirectoryPath);
            Polling.Instance.TimeInMinutes = 0.0005;
            Polling.InboxFilesDetected += delegate
            {
                pass = true;
            };
            Polling.Instance.Start();

            while (!pass)
            {
                if (pass)
                {
                    break;
                }
            }
        }
    }
}
