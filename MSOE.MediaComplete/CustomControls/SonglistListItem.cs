using System.Windows.Controls;
using System.Windows.Input;

namespace MSOE.MediaComplete.CustomControls
{
    public class SonglistListItem : SongListItem
    {
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            IsSelected = true;
            base.OnPreviewMouseRightButtonDown(e);
        }
        public FolderTreeViewItem ParentItem { private get; set; }

        public override string ToString()
        {

            return (string) Content;
        }

        public override string GetPath()
        {
            return ParentItem.GetPath() + Content;
        }

    }

}