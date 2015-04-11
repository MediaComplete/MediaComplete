using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MSOE.MediaComplete.Lib;
using consts = MSOE.MediaComplete.Lib.Constants;

namespace MSOE.MediaComplete.CustomControls
{
    public class FolderTreeViewItem : INotifyPropertyChanged
    {
        public FolderTreeViewItem()
		{
            Children = new ObservableCollection<FolderTreeViewItem>();
		}

        #region Properties

        /// <summary>
        /// Contains all folders contained within this folder.
        /// </summary>
        [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")] // Gets manually bound
        public ObservableCollection<FolderTreeViewItem> Children { get; private set; }

        /// <summary>
        /// The folder the current folder is in. 
        /// This value is null if it is the root
        /// </summary>
        public FolderTreeViewItem ParentItem { get; set; }

        /// <summary>
        /// The header content of this tree view item
        /// </summary>
        private object _header;
        public object Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged("Header");
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        private bool _isSelected;

        #endregion

        /// <summary>
        /// Just returns the Header property as a string
        /// </summary>
        /// <returns>A string representation of this FolderTreeViewItem</returns>
        public override string ToString()
        {
            return Header.ToString();
        }

        /// <summary>
        /// Used to recursively determine the folder's path
        /// </summary>
        /// <returns>string representation of path</returns>
        public string GetPath()
        {
            if (ParentItem != null)
            {
                return ParentItem.GetPath() + Header + Path.DirectorySeparatorChar;
            }
            return SettingWrapper.MusicDir;
            
        }

        #region INotifyPropertyChanged members

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged members
    }
}