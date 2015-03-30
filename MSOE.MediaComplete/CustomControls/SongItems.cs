using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MSOE.MediaComplete.CustomControls
{

    public abstract class AbstractSongItem : ListViewItem
    {
        public abstract string GetPath();

        public bool IsPlaying { get; set; }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.RegisterAttached(
        "IsPlaying", typeof(bool), typeof(AbstractSongItem), new PropertyMetadata(false));  
    }

    class PlaylistSongItem : AbstractSongItem
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


    public class LibrarySongItem : AbstractSongItem
    {
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            IsSelected = true;
            base.OnPreviewMouseRightButtonDown(e);
        }

        public FolderTreeViewItem ParentItem { private get; set; }

        public override string ToString()
        {

            return (string)Content;
        }

        public override string GetPath()
        {
            return ParentItem.GetPath() + Content;
        }
    }
}
