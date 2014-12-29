﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Test.Util;
using System.IO;

namespace MSOE.MediaComplete.Test
{
    [TestClass]
    public class ImporterTest
    {
        private DirectoryInfo _homeDir;
        private DirectoryInfo _importDir;
        private DirectoryInfo _testDir;

        [TestInitialize]
        public void Setup()
        {
            _testDir = FileHelper.CreateDirectory("ImporterTest");
            _homeDir = FileHelper.CreateDirectory("ImporterTest" + Path.DirectorySeparatorChar + "HomeDir");
            _importDir = FileHelper.CreateDirectory("ImporterTest" + Path.DirectorySeparatorChar + "ImportDir");
            SettingWrapper.SetHomeDir(_homeDir.FullName);
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
            Directory.Delete(_importDir.FullName, true);
        }

        [TestMethod, Timeout(30000)]
        public void Import_FileInUse_SkipAndNotify()
        {
            var fileInUse = FileHelper.CreateTestFile(_importDir.FullName);
            Task<ImportResults> task;
            using (fileInUse.OpenWrite())
            {
                task = new Importer(_homeDir.FullName).ImportFiles(new[] { fileInUse.FullName }, true);
                while (!task.IsCompleted)
                {
                }
            }

            Assert.AreEqual(1, task.Result.FailCount, "The locked file wasn't counted!");
            Assert.AreEqual(0, task.Result.NewFiles.Count, "The locked file was counted as a success!");
            Assert.AreEqual(0, _homeDir.GetFiles().Length, "There shouldn't be any files in the library!");
            Assert.IsTrue(fileInUse.Exists, "The source file doesn't exist!");
        }

        [TestMethod, Timeout(30000)]
        public void Import_FromLibrary_Exception()
        {
            var fileInLib = FileHelper.CreateTestFile(_homeDir.FullName);
            var task = new Importer(_homeDir.FullName).ImportFiles(new[] { fileInLib.FullName }, true);
            while (!task.IsCompleted)
            {
            }

            if (task.Exception == null)
                Assert.Fail("No exception occured!");

            Assert.IsInstanceOfType(task.Exception.InnerException, typeof(InvalidImportException), 
                "Exception was not an InvalidImportException!");
            Assert.IsTrue(fileInLib.Exists, "The source file doesn't exist!");
        }

        [TestMethod, Timeout(30000)]
        public void Import_FromAboveLibrary_Skips()
        {
            var newFile = FileHelper.CreateTestFile(_importDir.FullName);
            var oldFile = FileHelper.CreateMissingAlbumTestFile(_homeDir.FullName);
            var task = new Importer(_homeDir.FullName).ImportDirectory(_testDir.FullName, false);
            while (!task.IsCompleted)
            {
            }

            Assert.AreEqual(0, task.Result.FailCount, "There was a failed file!");
            Assert.AreEqual(1, task.Result.NewFiles.Count, "The file wasn't moved!");
            Assert.AreEqual(2, _homeDir.GetFiles().Length, "The new and/or old files aren't in the home dir!");
            Assert.IsTrue(oldFile.Exists, "Old file is gone!");
            Assert.IsFalse(newFile.Exists, "New file is still in the import dir!");
        }

        [TestMethod, Timeout(30000)]
        public void Import_FromMultiTieredDirs_GetsAll()
        {
            var childFile = FileHelper.CreateTestFile(_importDir.FullName);
            var parentFile = FileHelper.CreateMissingAlbumTestFile(_testDir.FullName);
            var task = new Importer(_homeDir.FullName).ImportDirectory(_testDir.FullName, false);
            while (!task.IsCompleted)
            {
            }

            Assert.AreEqual(0, task.Result.FailCount, "There was a failed file!");
            Assert.AreEqual(2, task.Result.NewFiles.Count, "The files weren't moved!");
            Assert.AreEqual(2, _homeDir.GetFiles().Length, "The files aren't in the home dir!");
            Assert.IsFalse(childFile.Exists, "Nested child file wasn't moved!");
            Assert.IsFalse(parentFile.Exists, "File in the root of the import wasn't moved!");
        }
    }
}