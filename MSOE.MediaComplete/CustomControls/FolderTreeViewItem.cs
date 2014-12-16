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
        /// <param name="path"></param>
        /// <returns>string representation of path</returns>
        private string GetPath(string path)
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

        /// <summary>
        /// Used to get the absolute path of the Folder
        /// </summary>
        /// <returns>string representation of path</returns>
        public string GetPath()
        {
            return GetPath("");
        }
    }
}