using System.Windows.Controls;

namespace MSOE.MediaComplete
{
    internal static class Utils
    {
        public static string FilePath(this TreeViewItem leaf)
        {
            var parentPath = "";
            var item = leaf.Parent as TreeViewItem;
            if (item != null)
            {
                parentPath = item.FilePath();
            }
            return parentPath + leaf.Header;
        }
    }
}