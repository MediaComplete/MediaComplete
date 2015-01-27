using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PollingTest
    {
        private static FileInfo _file;
        private const string DirectoryPath = "C:\\TESTinboxForLibrary\\";
        private const string Mp3FileName = "file.mp3";
        private const string WmaFileName = "file.wma";
        private const string WavFileName = "file.wav";

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
        public void CheckForSongMp3()
        {
            Directory.CreateDirectory(DirectoryPath);
            _file = new FileInfo(DirectoryPath + Mp3FileName);
            File.Create(_file.FullName).Close();

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
        /// <summary>
        /// tests that it calls the delegate
        /// </summary>
        [TestMethod]
        [Timeout(40000)]//Milliseconds
        public void CheckForSongWma()
        {
            Directory.CreateDirectory(DirectoryPath);
            _file = new FileInfo(DirectoryPath + WmaFileName);
            File.Create(_file.FullName).Close();

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
        /// <summary>
        /// tests that it calls the delegate
        /// </summary>
        [TestMethod]
        public void CheckForSongWav()
        {
            Directory.CreateDirectory(DirectoryPath);
            _file = new FileInfo(DirectoryPath + WavFileName);
            File.Create(_file.FullName).Close();

            var pass = false;
            SettingWrapper.SetInboxDir(DirectoryPath);
            Polling.Instance.TimeInMinutes = 0.0005;
            Polling.InboxFilesDetected += delegate
            {
                pass = true;
            };
            Polling.Instance.Start();

            var timer = new Timer(DoNothing);
            timer.Change(1000, Timeout.Infinite);

            if (!File.Exists(_file.FullName))
            {
                Assert.Fail("File does not exist in source directory - it should not have been moved.");
            }
        }

        private static void DoNothing(object o)
        {

        }
    }
}

