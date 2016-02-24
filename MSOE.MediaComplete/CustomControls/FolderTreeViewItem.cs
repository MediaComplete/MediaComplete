using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MediaComplete.Lib;
using consts = MediaComplete.Lib.Constants;

namespace MediaComplete.CustomControls
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
        /// Setting this value will also ensure it is correctly sorted within the new parent.
        /// </summary>
        private FolderTreeViewItem _parentItem;
        public FolderTreeViewItem ParentItem
        {
            get { return _parentItem; }
            set
            {
                _parentItem = value;
                var i = 0;
                while (i < _parentItem.Children.Count)
                {
                    var cmp = String.Compare(_parentItem.Children[i].Header.ToString(), Header.ToString(), StringComparison.Ordinal);
                    if (cmp >= 0)
                        break;
                    i++;
                }
                _parentItem.Children.Insert(i, this);
            }
        }

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
            return SettingWrapper.MusicDir.FullPath;
            
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