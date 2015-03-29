namespace MSOE.MediaComplete.CustomControls
{
    public class PlaylistListItem : SongListItem
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