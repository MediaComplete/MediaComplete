using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MSOE.MediaComplete.CustomControls
{
    internal class FolderTreeViewItem : TreeViewItem
    {
        public FolderTreeViewItem()
		{
            Children = new ObservableCollection<FolderTreeViewItem>();
            HasParent = true;
		}


        public ObservableCollection<FolderTreeViewItem> Children { get; set; }
        public FolderTreeViewItem ParentItem { get; set; }
        public bool HasParent { get; set; }
        public override string ToString()
        {
            return (string) Header;
        }
        public string GetPath(string path)
        {
            if (HasParent)
            {
                path = ParentItem.GetPath(path) + Header + "\\";
            }
            else
            {
                path = Header + path;
            }
            return path;
        }

        public string GetPath()
        {
            return GetPath("");
        }
    }
}