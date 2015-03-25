using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using MSOE.MediaComplete.Lib.Metadata;
using TagLib;
using TextBox = System.Windows.Controls.TextBox;

namespace MSOE.MediaComplete
{
    public partial class MainWindow
    {
        private IEnumerable<TextBox> ClearBoxes()
        {
            var boxes = new TextBox[8];
            boxes[0] = SongTitle;
            boxes[1] = Album;
            boxes[2] = Artist;
            boxes[3] = SuppArtist;
            boxes[4] = Genre;
            boxes[5] = Track;
            boxes[6] = Year;
            boxes[7] = Rating;

            foreach (var box in boxes)
            {
                while (box.CanUndo)
                    box.Undo();
            }
            return boxes;
        }
        private void PopulateMetadataForm()
        {
            var boxes = ClearBoxes();
            var initalAttributes = new Dictionary<MetaAttribute, string>
            {
                {MetaAttribute.SongTitle, null},
                {MetaAttribute.Album, null},
                {MetaAttribute.Artist, null},
                {MetaAttribute.SupportingArtist, null},
                {MetaAttribute.Genre, null},
                {MetaAttribute.TrackNumber, null},
                {MetaAttribute.Year, null},
                {MetaAttribute.Rating, null},
                {MetaAttribute.AlbumArt, null}
            };
            var finalAttributes = new Dictionary<MetaAttribute, string>();
            foreach ( SongTreeViewItem item in SongTree.SelectedItems)
            {
                try
                {
                    var song = File.Create(item.GetPath());
                    foreach (var metaAttribute in Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().Where(metaAttribute => !finalAttributes.ContainsKey(metaAttribute)))
                    {
                        if (initalAttributes[metaAttribute] == null)
                            initalAttributes[metaAttribute] = song.GetAttribute(metaAttribute);
                        else if (!initalAttributes[metaAttribute].Equals(song.GetAttribute(metaAttribute)))
                        {
                            initalAttributes[metaAttribute] = "-1";
                        }
                        if (initalAttributes[metaAttribute] == null) continue;
                        if (!initalAttributes[metaAttribute].Equals("-1")) continue;
                        finalAttributes.Add(metaAttribute, initalAttributes[metaAttribute]);
                        initalAttributes.Remove(metaAttribute);
                    }
                }
                catch (CorruptFileException)
                {
                    StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusBarHandler.StatusIcon.Error);
                }
            }
            foreach (var attribute in initalAttributes.Where(attribute => !finalAttributes.ContainsKey(attribute.Key)))
            {
                finalAttributes.Add(attribute.Key, initalAttributes[attribute.Key]);
            }

            SongTitle.Text = finalAttributes[MetaAttribute.SongTitle] == "-1" ?  Resources["VariousSongs"].ToString() : finalAttributes[MetaAttribute.SongTitle];
            Album.Text = finalAttributes[MetaAttribute.Album] == "-1" ? Resources["VariousAlbums"].ToString() : finalAttributes[MetaAttribute.Album];
            Artist.Text = finalAttributes[MetaAttribute.Artist] == "-1" ?  Resources["VariousArtists"].ToString() : finalAttributes[MetaAttribute.Artist];
            SuppArtist.Text = finalAttributes[MetaAttribute.SupportingArtist] == "-1" ?  Resources["VariousArtists"].ToString() : finalAttributes[MetaAttribute.SupportingArtist];
            Genre.Text = finalAttributes[MetaAttribute.Genre] == "-1" ?  Resources["VariousGenres"].ToString() : finalAttributes[MetaAttribute.Genre];
            Track.Text = finalAttributes[MetaAttribute.TrackNumber] == "-1" ?  Resources["VariousTrackNumbers"].ToString() : finalAttributes[MetaAttribute.TrackNumber];
            Year.Text = finalAttributes[MetaAttribute.Year] == "-1" ?  Resources["VariousYear"].ToString() : finalAttributes[MetaAttribute.Year];
            Rating.Text = finalAttributes[MetaAttribute.Rating] == "-1" ?  Resources["VariousRatings"].ToString() : finalAttributes[MetaAttribute.Rating];
            
        }


        private void ClearDetailPane()
        {
            SongTitle.Text = "";
            Album.Text = "";
            Artist.Text = "";
            SuppArtist.Text = "";
            Rating.Text = "";
            Track.Text = "";
            Year.Text = "";
            Genre.Text = "";

            EditCancelButton.Content = Resources["EditButton"].ToString();

        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {

            if (EditCancelButton.Content.Equals(Resources["EditButton"].ToString()) && SongTree.SelectedItems.Count > 0)
            {
                EditCancelButton.Content = Resources["CancelButton"].ToString();
                ToggleReadOnlyFields(false);
            }
            else if (EditCancelButton.Content.Equals(Resources["CancelButton"].ToString()))
            {
                foreach (var changedBox in _changedBoxes)
                {
                    while (changedBox.CanUndo)
                    {
                        changedBox.Undo();
                    }
                    changedBox.Redo();
                    changedBox.LockCurrentUndoUnit();
                }
                if(_changedBoxes.Count != 0) PopulateMetadataForm();
                _changedBoxes.Clear();
                ToggleReadOnlyFields(true);
                EditCancelButton.Content = Resources["EditButton"].ToString();
            }
        }

        private void FormCheck()
        {
            if (EditCancelButton.Content.Equals(Resources["CancelButton"].ToString()))
            {
                foreach (var changedBox in _changedBoxes)
                {
                    while (changedBox.CanUndo)
                    {
                        changedBox.Undo();
                    }
                    changedBox.Redo();
                    changedBox.LockCurrentUndoUnit();
                }
                _changedBoxes.Clear();
                ToggleReadOnlyFields(true);
                EditCancelButton.Content = Resources["EditButton"].ToString();
                PopulateMetadataForm();
            }
        }
		
        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongTitle.IsReadOnly) return;
            EditCancelButton.Content = Resources["EditButton"].ToString();
            ToggleReadOnlyFields(true);
            foreach (SongTreeViewItem item in SongTree.SelectedItems)
            {
                try
                {
                    var song = File.Create(item.GetPath());
                    foreach (var changedBox in _changedBoxes)
                    {
                        if (changedBox.Equals(SongTitle))
                        {
                            song.SetAttribute(MetaAttribute.SongTitle, changedBox.Text);
                        }
                        else if (changedBox.Equals(Album))
                        {
                            song.SetAttribute(MetaAttribute.Album, changedBox.Text);
                        }
                        else if (changedBox.Equals(Artist))
                        {
                            song.SetAttribute(MetaAttribute.Artist, changedBox.Text);
                        }
                        else if (changedBox.Equals(Genre))
                        {
                            song.SetAttribute(MetaAttribute.Genre, changedBox.Text);
                        }
                        else if (changedBox.Equals(Track))
                        {
                            song.SetAttribute(MetaAttribute.TrackNumber, changedBox.Text);
                        }
                        else if (changedBox.Equals(Year))
                        {
                            song.SetAttribute(MetaAttribute.Year, changedBox.Text);
                        }
                        else if (changedBox.Equals(Rating))
                        {
                            song.SetAttribute(MetaAttribute.Rating, changedBox.Text);
                        }
                        else if (changedBox.Equals(SuppArtist))
                        {
                            song.SetAttribute(MetaAttribute.SupportingArtist, changedBox.Text);
                        }
                    }
                }
                catch (CorruptFileException)
                {
                    StatusBarHandler.Instance.ChangeStatusBarMessage("Save-Error", StatusBarHandler.StatusIcon.Error);
                }
            }
            _changedBoxes.Clear();
            PopulateMetadataForm();
        }

        private void ToggleReadOnlyFields(bool toggle)
        {
            SongTitle.IsReadOnly = toggle;
            Album.IsReadOnly = toggle;
            SuppArtist.IsReadOnly = toggle;
            Artist.IsReadOnly = toggle;
            Genre.IsReadOnly = toggle;
            Year.IsReadOnly = toggle;
            Rating.IsReadOnly = toggle;
            Track.IsReadOnly = toggle;
        }

    }
}
