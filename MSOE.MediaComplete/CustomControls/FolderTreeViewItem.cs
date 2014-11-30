using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MSOE.MediaComplete.CustomControls
{
    internal class FolderTreeViewItem : TreeViewItem
    {
        public FolderTreeViewItem()
		{
            Children = new ObservableCollection<FolderTreeViewItem>();
            Root = false;
		}


        public ObservableCollection<FolderTreeViewItem> Children { get; set; }
        public FolderTreeViewItem ParentItem { get; set; }
        public bool Root { get; set; }
        public override string ToString()
        {
            return (string) Header;
        }
    }
}