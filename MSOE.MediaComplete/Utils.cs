using System.Windows.Controls;

namespace MSOE.MediaComplete
{
    internal static class Utils
    {
        public static string FilePath(this TreeViewItem leaf)
        {
            var parentPath = "";
            if (leaf.Parent is TreeViewItem)
            {
                parentPath = (leaf.Parent as TreeViewItem).FilePath();
            }
            return parentPath + leaf.Header;
        }
    }
}