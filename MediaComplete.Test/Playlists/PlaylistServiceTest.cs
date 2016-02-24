using NUnit.Framework;

namespace MediaComplete.Test.Playlists
{
    [TestFixture]
    [Ignore]
    public class PlaylistServiceTest
    {

        #region GetAllPlaylists()
        [Test]
        public void GetAll_FindsM3Us_IgnoresOtherFiles()
        {
            // Mocked directory "contains" 2 M3U files, an MP3, and a TXT. 
            // Verify that only the M3Us get returned.
        }

        [Test]
        public void GetAll_EmptyDir_ReturnsEmptyList()
        {
            // Mocked directory is empty. Verify that returned list is empty.
        }
        #endregion

        #region GetPlaylist(string name)
        [Test]
        public void GetPlaylist_AlreadyExists_ReturnsPlaylist()
        {
            // "Happy path" for GetPlaylist.
        }

        [Test]
        public void GetPlaylist_EmptyName_ThrowsException()
        {
            // Call GetPlaylist with an empty string name.
            // Verify that an ArgumentException is thrown
        }

        [Test]
        public void GetPlaylist_NullName_ThrowsException()
        {

        }

        [Test]
        public void GetPlaylist_DoesNotExist_ReturnsNull()
        {
            
        }
        #endregion

        #region CreatePlaylist()
        [Test]
        public void CreatePlaylist_NoNameOnce_ReturnsNewPlaylist()
        {
            
        }

        [Test]
        public void CreatePlaylist_NoNameMultiple_ReturnsUniquePlaylists()
        {

        }
        #endregion

        #region CreatePlaylist(string name)
        [Test]
        public void CreatePlaylist_NameAlreadyExists_ThrowsException()
        {

        }

        [Test]
        public void CreatePlaylist_EmptyName_ThrowsException()
        {

        }

        [Test]
        public void CreatePlaylist_NullName_ThrowsException()
        {

        }

        [Test]
        public void CreatePlaylist_ValidName_ReturnsNewPlaylist()
        {

        }
        #endregion
    }
}
