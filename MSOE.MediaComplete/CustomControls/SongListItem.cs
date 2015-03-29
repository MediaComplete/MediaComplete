using System.Windows.Controls;

namespace MSOE.MediaComplete.CustomControls
{
    public abstract class SongListItem : ListViewItem
    {
        public abstract string GetPath();

    }
}