using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Sorting;
using MSOE.MediaComplete.Test.Util;

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
            SettingWrapper.SetHomeDir(_homeDir.FullName);
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
            var sourceFile = FileHelper.CreateTestFile(_homeDir.FullName);
            sourceFile.MoveTo(sourceFile.FullName + ".test.mp3");
            var normalFilePath = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                                 Path.DirectorySeparatorChar + "The Money Store";
            FileHelper.CreateTestFile(normalFilePath);

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(0, subject.UnsortableCount, "Sorter shouldn't have invalid files!");
            Assert.AreEqual(0, subject.MoveCount, "Sorter shouldn't move any files!");
            Assert.AreEqual(1, subject.DupCount, "Sorter didn't plan to delete dup file!");
            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName + ".test.mp3",
                ((Sorter.DeleteAction) subject.Actions[0]).Target.FullName, "Didn't plan to delete the right file.");

            var task = subject.PerformSort();
            while (!task.IsCompleted)
            {
            }

            Assert.IsFalse(sourceFile.Exists, "Source file wasn't cleaned up!");
            Assert.IsFalse(new FileInfo(normalFilePath + Path.DirectorySeparatorChar + sourceFile.Name).Exists, "Source file shouldn't have been copied!");
        }

        /// <summary>
        /// If there's a corrupeted MP3 file, it should just be skipped over.
        /// </summary>
        [TestMethod]
        public void Sort_InvalidFile_Skips()
        {
            FileHelper.CreateInvalidTestFile(_homeDir.FullName);
            FileHelper.CreateTestFile(_homeDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" + 
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter didn't count up the invalid file!");
            Assert.AreEqual(1, subject.MoveCount, "Sorter didn't plan to move the valid file!");
            Assert.AreEqual(0, subject.DupCount, "Sorter didn't plan to move the valid file!");
            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName, 
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
            FileHelper.CreateMissingAlbumTestFile(_homeDir.FullName);
            var noAblbumDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" + 
                Path.DirectorySeparatorChar + Util.Constants.MissingAlbumMp3FileName;
            FileHelper.CreateTestFile(_homeDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter should still flag the file!");
            Assert.AreEqual(2, subject.MoveCount, "Both files should move!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName,
                ((Sorter.MoveAction)subject.Actions[1]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[1]).Dest.FullName, 
                "Didn't plan to move normal file to the right destination.");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.MissingAlbumMp3FileName,
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
            FileHelper.CreateMissingArtistTestFile(_homeDir.FullName);
            FileHelper.CreateTestFile(_homeDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(1, subject.UnsortableCount, "Sorter should still flag the file!");
            Assert.AreEqual(1, subject.MoveCount, "Only the normal file should move!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(_homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName,
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
            FileHelper.CreateTestFile(oldDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(0, subject.UnsortableCount, "The file should be sortable!");
            Assert.AreEqual(1, subject.MoveCount, "The file should be moved!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(oldDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName, 
                "Didn't plan to move normal file to the right destination.");

            var task = subject.PerformSort();
            while (!task.IsCompleted)
            {
            }

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
            FileHelper.CreateTestFile(oldDir.FullName);
            FileHelper.CreateNonMp3TestFile(oldDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var subject = new Sorter(_homeDir, GetNormalSettings());

            Assert.AreEqual(0, subject.UnsortableCount, "The file should be sortable!");
            Assert.AreEqual(1, subject.MoveCount, "The file should be moved!");
            Assert.AreEqual(0, subject.DupCount, "There shouldn't be any dups!");

            Assert.AreEqual(oldDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName,
                ((Sorter.MoveAction)subject.Actions[0]).Source.FullName, "Didn't plan to move the normal file.");
            Assert.AreEqual(normalFileDest, ((Sorter.MoveAction)subject.Actions[0]).Dest.FullName,
                "Didn't plan to move normal file to the right destination.");

            var task = subject.PerformSort();
            while (!task.IsCompleted)
            {
            }

            Assert.IsTrue(oldDir.Exists, "Old directory should still exist!");
            Assert.IsTrue(new FileInfo(normalFileDest).Exists, "File wasn't moved!");
        }

        /// <summary>
        /// Make sure that imports trigger sorting operations on the new files.
        /// </summary>
        [TestMethod, Timeout(30000)]
        public void Import_CausesSort_IgnoresOldFiles()
        {
            SettingWrapper.SetIsSorting(true);
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(null, null);// Force the static initializer to fire.
            var decoyFile = FileHelper.CreateTestFile(_homeDir.FullName); // Deliberately put an unsorted file in
            decoyFile.MoveTo(decoyFile.FullName + ".decoy.mp3");
            FileHelper.CreateTestFile(_importDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + "Death Grips" +
                Path.DirectorySeparatorChar + "The Money Store" + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var task = new Importer(_homeDir.FullName ).ImportDirectory(_importDir.FullName, true);
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
        /// Make sure that imports trigger sorting operations on the new files.
        /// </summary>
        [TestMethod, Timeout(30000)]
        public void Import_NoSort_IgnoresNewFiles()
        {
            SettingWrapper.SetIsSorting(false);
            // ReSharper disable once ObjectCreationAsStatement
            new Sorter(null, null); // Force the static initializer to fire.
            var decoyFile = FileHelper.CreateTestFile(_homeDir.FullName); // Deliberately put an unsorted file in
            decoyFile.MoveTo(decoyFile.FullName + ".decoy.mp3");
            FileHelper.CreateTestFile(_importDir.FullName);
            var normalFileDest = _homeDir.FullName + Path.DirectorySeparatorChar + Util.Constants.ValidMp3FileName;

            var task = new Importer(_homeDir.FullName).ImportDirectory(_importDir.FullName, true);
            while (!task.IsCompleted)
            {

            }
            // Need to poll for the file, since we don't have a way of monitoring the sorter directly.
            while (!new FileInfo(normalFileDest).Exists)
            {
            }

            Assert.IsTrue(decoyFile.Exists);
        }

        public static SortSettings GetNormalSettings()
        {
            return new SortSettings
            {
                SortOrder = new List<MetaAttribute> { MetaAttribute.Artist, MetaAttribute.Album }
            };
        }
    }
}
