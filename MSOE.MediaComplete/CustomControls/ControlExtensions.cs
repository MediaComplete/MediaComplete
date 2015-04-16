namespace MSOE.MediaComplete.CustomControls
{
    public static class ControlExtensions
    {
        public static bool IsSelectedRecursive(this FolderTreeViewItem item)
        {
            if (item == null)
            {
                return false;
            }
            return item.IsSelected || item.ParentItem.IsSelectedRecursive();
        }
    }
}
