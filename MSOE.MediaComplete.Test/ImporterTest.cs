using System.Threading.Tasks;
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

        [TestInitialize]
        public void Setup()
        {
            _homeDir = FileHelper.CreateDirectory("ImporterTestHomeDir");
            _importDir = FileHelper.CreateDirectory("ImporterTestImportDir");
            SettingWrapper.SetHomeDir(_homeDir.FullName);
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
            Directory.Delete(_importDir.FullName, true);
        }

        [TestMethod]
        public void Import_FileInUse_SkipAndNotify()
        {
            var fileInUse = FileHelper.CreateTestFile(_importDir.FullName);
            Task<ImportResults> task;
            using (fileInUse.OpenWrite())
            {
                task = new Importer().ImportFiles(new[] { fileInUse.FullName }, true);
                while (!task.IsCompleted)
                {
                }
            }

            Assert.AreEqual(1, task.Result.FailCount, "The locked file wasn't counted!");
            Assert.AreEqual(0, task.Result.NewFiles.Count, "The locked file was counted as a success!");
            Assert.AreEqual(0, _homeDir.GetFiles().Length, "There shouldn't be any files in the library!");
            Assert.IsTrue(new FileInfo(Util.Constants.ReadOnlyFile).Exists, "The source file doesn't exist!");
        }

        [TestMethod]
        public void Import_AccessDenied_SkipAndNotify()
        {
            var task = new Importer().ImportFiles(new[] { Util.Constants.ReadOnlyFile }, true);
            while (!task.IsCompleted)
            {
            }
            Assert.AreEqual(1, task.Result.FailCount, "The locked file wasn't counted!");
            Assert.AreEqual(0, task.Result.NewFiles.Count, "The locked file was counted as a success!");
            Assert.AreEqual(0, _homeDir.GetFiles().Length, "There shouldn't be any files in the library!");
            Assert.IsTrue(new FileInfo(Util.Constants.ReadOnlyFile).Exists, "The source file doesn't exist!");
        }

        [TestMethod]
        public void Import_FromLibrary_NotifyAndAbort()
        {
        }

        [TestMethod]
        public void Import_FromAboveLibrary_Skips()
        {
        }

        [TestMethod]
        public void Import_FromMultiTieredDirs_GetsAll()
        {
        }
    }
}
