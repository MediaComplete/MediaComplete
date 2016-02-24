using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaComplete.Lib.Library;

namespace MediaComplete.CustomControls
{

    public class SongListItem : ListViewItem
    {
        public AbstractSong Data { get; set; }

        public FolderTreeViewItem ParentItem { get; set; }

        public bool IsPlaying
        {
            get { return (bool) GetValue(IsPlayingProperty); }
            set { SetValue(IsPlayingProperty, value); }
        }
        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            "IsPlaying", typeof(bool), typeof(SongListItem), new PropertyMetadata(false));

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            IsSelected = true;
            base.OnPreviewMouseRightButtonDown(e);
        }

        public override string ToString()
        {
            return (string)Content;
        }
    }
}
