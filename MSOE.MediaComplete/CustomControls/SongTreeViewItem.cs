using System.Windows.Controls;
using System.Windows.Input;

namespace MSOE.MediaComplete.CustomControls
{
    internal class SongTreeViewItem : TreeViewItem
    {
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            IsSelected = true;
            base.OnPreviewMouseRightButtonDown(e);
        }
    }
}