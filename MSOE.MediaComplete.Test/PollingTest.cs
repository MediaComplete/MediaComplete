using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class PollingTest
    {
        private static bool _pass = false;
        private static FileInfo _file;

        [ClassInitialize]
        public void Before()
        {
            _file = new FileInfo("C:\\inboxForLibrary\\file.mp3");
            File.Create(_file.FullName);
        }

        [ClassCleanup]
        public void After()
        {
            if (File.Exists(_file.FullName))
            {
                File.Delete(_file.FullName);
            }
        }

        /// <summary>
        /// tests that it calls the delegate
        /// </summary>
        [TestMethod]
        [Timeout(40000)]//Milliseconds
        public static void OnTimerFinishedTest()
        {
            Polling.Instance.inboxDir = _file.DirectoryName;
            Polling.Instance.TimeInMinutes = 0.5;
            Polling.InboxFilesDetected += RunMe;
            Polling.Instance.Start();
        }

        public static void RunMe(IEnumerable<FileInfo> files)
        {
            _pass = true;
        }
    }
}
