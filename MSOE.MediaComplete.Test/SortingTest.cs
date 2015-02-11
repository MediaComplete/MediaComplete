using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Import;
using MSOE.MediaComplete.Lib.Metadata;
using MSOE.MediaComplete.Lib.Sorting;
using MSOE.MediaComplete.Test.Util;
using Constants = MSOE.MediaComplete.Test.Util.Constants;

namespace MSOE.MediaComplete.Test
{
    /// <summary>
    /// Tests for <seealso cref="Sorter"/>
    /// </summary>
    [TestClass]
    public class SortingTest
    {
        private DirectoryInfo _homeDir;
        private DirectoryInfo _importDir;

        [TestInitialize]
        public void Setup()
        {
            _homeDir = FileHelper.CreateDirectory("SortingTestHomeDir");
            _importDir = FileHelper.CreateDirectory("SortingTestImportDir");
            SettingWrapper.HomeDir = _homeDir.FullName;
        }

        [TestCleanup]
        public void TearDown()
        {
            Directory.Delete(_homeDir.FullName, true);
            Directory.Delete(_importDir.FullName, true);
        }

        /// <summary>
        /// If a file that contains the same song already exists, then the original should just be deleted (no movement should take place).
        /// </summary>
        [TestMethod]
        public void Sort_FileAlreadyExsists_LeavesOld()
        {
            var sourceFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            sourceFile.MoveTo(sourceFile.FullName + ".test.mp3");
            var normalFilePath = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                                 Path.DirectorySeparatorChar + "The Money Store";
            FileHelper.CreateFile(new DirectoryInfo(normalFilePath), Constants.FileTypes.ValidMp3);

            var subject = new Sorter(GetNormalSettings());
            var task  = subject.CalculateActionsAsync();
            SpinWait.SpinUntil(() => task.IsCompleted);

            Assert.AreEqual(0, subject.UnsortableCount, "Sorter shouldn't have invalid files!");
            Assert.AreEqual(0, subject.MoveCount, "Sorter shouldn't move any files!");
            Assert.AreEqual(1, subject.DupCount, "Sorter didn't plan to delete dup file!");
            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1 + ".test.mp3",
                ((Sorter.DeleteAction)subject.Actions[0]).Target.FullName, "Didn't plan to delete the right file.");

            var sortTask = subject.PerformSort();

            var sysTask = Task.Run(() => sortTask.Lock.WaitAsync());
            sysTask.Wait();

            Assert.IsFalse(sourceFile.Exists, "Source file wasn't cleaned up!");
            Assert.IsFalse(new FileInfo(normalFilePath + Path.DirectorySeparatorChar + sourceFile.Name).Exists, "Source file shouldn't have been copied!");
        }

        /// <summary>
        /// If there's a corrupeted MP3 file, it should just be skipped over.
        /// </summary>
        [TestMethod]
        public void Sort_InvalidFile_Skips()
        {
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.Invalid);
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var subject = new Sorter(GetNormalSettings());
            var task = subject.CalculateActionsAsync();
            task.Wait();

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter didn't count up the invalid file!");
            Assert.AreEqual(1, subject.MoveCount, "Sorter didn't plan to move the valid file!");
            Assert.AreEqual(0, subject.DupCount, "Sorter didn't plan to move the valid file!");
            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the right file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move to the right destination.");
        }

        /// <summary>
        /// If a file has partial metadata, it should be able to move as far as possible.
        /// </summary>
        [TestMethod]
        public void Sort_NoAlbum_MovesDown1()
        {
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.MissingAlbum);
            var noAblbumDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.MissingAlbum].Item1;
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var subject = new Sorter(GetNormalSettings());
            var task = subject.CalculateActionsAsync();
            SpinWait.SpinUntil(() => task.IsCompleted);

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter should still flag the file!");
            Assert.AreEqual(2, subject.MoveCount, "Both files should move!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1,
                ((Sorter.MoveAction)subject.Actions[1]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[1]).Dest.FullName,
                "Didn't plan to move normal file to the right destination.");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.MissingAlbum].Item1,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the missing album file.");
            Assert.AreEqual(noAblbumDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move missing album file to the right destination.");
        }

        /// <summary>
        /// If the file is missing the first sort criteria, it should just stay put.
        /// </summary>
        [TestMethod]
        public void Sort_NoArtist_StaysPut()
        {
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.MissingArtist);
            FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var subject = new Sorter(GetNormalSettings());
            var task = subject.CalculateActionsAsync();
            SpinWait.SpinUntil(() => task.IsCompleted);

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter should still flag the file!");
            Assert.AreEqual(1, subject.MoveCount, "Only the normal file should move!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move normal file to the right destination.");
        }

        /// <summary>
        /// Make sure empty directories get cleaned up.
        /// </summary>
        [TestMethod]
        public void Sort_ChangeDir_OldDirDeleted()
        {
            var oldDir = Directory.CreateDirectory(_homeDir.FullName + Path.DirectorySeparatorChar + "oldDir");
            FileHelper.CreateFile(oldDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var subject = new Sorter(GetNormalSettings());
            var task = subject.CalculateActionsAsync();
            SpinWait.SpinUntil(() => task.IsCompleted);

            Assert.AreEqual(0, subject.UnsortableCount, "The file should be sortable!");
            Assert.AreEqual(1, subject.MoveCount, "The file should be moved!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(oldDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move normal file to the right destination.");

            var sortTask = subject.PerformSort();
            var sysTask = Task.Run(() => sortTask.Lock.WaitAsync());
            sysTask.Wait();

            Assert.IsFalse(oldDir.Exists, "Old directory didn't get cleaned up!");
            Assert.IsTrue(new FileInfo(normalFileDest).Exists, "File wasn't moved!");
        }

        /// <summary>
        /// Directories that still contain files (even if they're not MP3s) should be left alone.
        /// </summary>
        [TestMethod]
        public void Sort_DirHasTxt_NotDeleted()
        {
            var oldDir = Directory.CreateDirectory(_homeDir.FullName + Path.DirectorySeparatorChar + "oldDir");
            FileHelper.CreateFile(oldDir, Constants.FileTypes.ValidMp3);
            FileHelper.CreateFile(oldDir, Constants.FileTypes.NonMusic);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var subject = new Sorter(GetNormalSettings());
            var task = subject.CalculateActionsAsync();
            SpinWait.SpinUntil(() => task.IsCompleted);

            Assert.AreEqual(0, subject.UnsortableCount, "The file should be sortable!");
            Assert.AreEqual(1, subject.MoveCount, "The file should be moved!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(oldDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move normal file to the right destination.");

            var sortTask = subject.PerformSort();
            var sysTask = Task.Run(() => sortTask.Lock.WaitAsync());
            sysTask.Wait();

            Assert.IsTrue(oldDir.Exists, "Old directory should still exist!");
            Assert.IsTrue(new FileInfo(normalFileDest).Exists, "File wasn't moved!");
        }

        /// <summary>
        /// Make sure that imports trigger sorting operations on the new files.
        /// </summary>
        [TestMethod, Timeout(30000)]
        public void Import_CausesSort_IgnoresOldFiles()
        {
            SettingWrapper.IsSorting = true;
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(null);// Force the static initializer to fire.
            var decoyFile = FileHelper.CreateFile(_homeDir, Constants.FileTypes.ValidMp3); // Deliberately put an unsorted file in

            decoyFile.MoveTo(decoyFile.FullName + ".decoy.mp3");
            FileHelper.CreateFile(_importDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var task = new Importer(_homeDir.FullName).ImportDirectoryAsync(_importDir.FullName, true);
            while (!task.IsCompleted)
            {

            }
            // Need to poll for the file, since we don't have a way of monitoring the sorter directly.
            while (!new FileInfo(normalFileDest).Exists)
            {
            }

            Assert.IsTrue(decoyFile.Exists);
        }

        /// <summary>
        /// Make sure that imports trigger does not trigger a sort.
        /// </summary>
        [TestMethod, Timeout(30000)]
        public void Import_NoSort_IgnoresNewFiles()
        {
            SettingWrapper.IsSorting = false;
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(null); // Force the static initializer to fire.
            FileHelper.CreateFile(_importDir, Constants.FileTypes.ValidMp3);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + Constants.TestFiles[Constants.FileTypes.ValidMp3].Item1;

            var task = new Importer(_homeDir.FullName).ImportDirectoryAsync(_importDir.FullName, true);
            while (!task.IsCompleted)
            {

            }
            Thread.Sleep(200);

            Assert.IsTrue(new FileInfo(normalFileDest).Exists);
        }

        public SortSettings GetNormalSettings()
        {
            return new SortSettings
            {
                SortOrder = new List<MetaAttribute> { MetaAttribute.Artist, MetaAttribute.Album },
                Root = _homeDir
            };
        }
    }
}
