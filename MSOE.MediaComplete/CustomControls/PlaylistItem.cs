
namespace MSOE.MediaComplete.CustomControls
{
    public class PlaylistItem : AbstractSongItem
    {
        public string Path { private get; set; }
        public void SetPath(string s)
        {
            Path = s;
        }
        public override string GetPath()
        {
            return Path;
        }

    }
}