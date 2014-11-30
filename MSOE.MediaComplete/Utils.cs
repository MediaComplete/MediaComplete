using System.Windows.Controls;
using MSOE.MediaComplete.CustomControls;

namespace MSOE.MediaComplete
{
    internal static class Utils
    {
        public static string FilePath(this TreeViewItem leaf)
        {
            var parentPath = "";
            if (leaf is SongTreeViewItem)
            {
                var songLeaf = (SongTreeViewItem) leaf;
                var temp = (songLeaf.ParentItem as FolderTreeViewItem);
                if (temp != null) { 
                    parentPath = temp.FilePath();
                }
            }
            else if (leaf is FolderTreeViewItem)
            {
                var folderLeaf = (FolderTreeViewItem)leaf;
                var temp = (folderLeaf.ParentItem as FolderTreeViewItem);
                if (temp != null)
                {
                    parentPath = temp.FilePath();
                }
            }
            return parentPath + "\\" + leaf.Header;
        }
    }
}