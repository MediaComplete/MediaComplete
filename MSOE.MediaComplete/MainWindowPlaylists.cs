using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSOE.MediaComplete.Lib.Playing;
using MSOE.MediaComplete.Lib.Playlists;

namespace MSOE.MediaComplete
{
    public partial class MainWindow
    {
        public ObservableCollection<Playlist> Playlists { get; private set; }

        private void InitPlaylists()
        {
            Playlists = new ObservableCollection<Playlist>(PlaylistService.GetAllPlaylists());
            FolderTreeViewContextMenuPlaylists.Collection = Playlists;
        }
    }
}
