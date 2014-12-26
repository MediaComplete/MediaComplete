using System.Collections.ObjectModel;
using System.Windows.Controls;
using MSOE.MediaComplete.Lib;

namespace MSOE.MediaComplete.CustomControls
{
    internal class FolderTreeViewItem : TreeViewItem
    {
        public FolderTreeViewItem()
		{
            Children = new ObservableCollection<FolderTreeViewItem>();
            HasParent = true;
		}
        
        /// <summary>
        /// Contains all folders contained within this folder.
        /// </summary>
        public ObservableCollection<FolderTreeViewItem> Children { get; set; }

        /// <summary>
        /// The folder the current folder is in. 
        /// This value is null if it is the root
        /// </summary>
        public FolderTreeViewItem ParentItem { get; set; }

        public bool HasParent { get; set; }
       
        public override string ToString()
        {
            return (string) Header;
        }

        /// <summary>
        /// Used to recursively determine the folder's path
        /// </summary>
        /// <returns>string representation of path</returns>
        public string GetPath()
        {
            if (HasParent)
            {
                return ParentItem.GetPath() + Header + "\\";
            }
            else
            {
                return SettingWrapper.GetHomeDir();
            }
        }
    }
}