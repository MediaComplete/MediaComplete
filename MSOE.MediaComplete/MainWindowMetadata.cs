using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using MSOE.MediaComplete.CustomControls;
using MSOE.MediaComplete.Lib;
using TagLib;
using TextBox = System.Windows.Controls.TextBox;

namespace MSOE.MediaComplete
{
    public partial class MainWindow
    {
        private void PopulateMetadataForm()
        {
            //Clear undo stack of each box
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
            if (SongTree.SelectedItems.Count == 1)
            {
                try
                {
                    var song = File.Create(((SongTreeViewItem) SongTree.SelectedItems[0]).GetPath());
                    SongTitle.Text = song.GetSongTitle();
                    Album.Text = song.GetAlbum();
                    Artist.Text = song.GetArtist();
                    SuppArtist.Text = song.GetSupportingArtist();
                    Genre.Text = song.GetGenre();
                    Track.Text = song.GetTrack();
                    Year.Text = song.GetYear();
                    Rating.Text = song.GetRating().Equals("-1") ? "" : song.GetRating();
                    StatusBarHandler.Instance.ChangeStatusBarMessage("", StatusBarHandler.StatusIcon.None);
                }
                catch (CorruptFileException)
                {
                    StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusBarHandler.StatusIcon.Error);
                }
            }
            else
            {
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
                        foreach (MetaAttribute metaAttribute in Enum.GetValues(typeof(MetaAttribute)).Cast<MetaAttribute>().Where(metaAttribute => !finalAttributes.ContainsKey(metaAttribute)))
                        {
                            if (initalAttributes[metaAttribute] == null)
                                initalAttributes[metaAttribute] = GetValue(metaAttribute, song);
                            else if (!initalAttributes[metaAttribute].Equals(GetValue(metaAttribute,song)))
                            {
                                initalAttributes[metaAttribute] = "-1";
                            }

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
                SongTitle.Text = finalAttributes[MetaAttribute.SongTitle] == "-1" ? "Various Songs" : finalAttributes[MetaAttribute.SongTitle];
                Album.Text = finalAttributes[MetaAttribute.Album] == "-1" ? "Various Albums" : finalAttributes[MetaAttribute.Album];
                Artist.Text = finalAttributes[MetaAttribute.Artist] == "-1" ? "Various Artists" : finalAttributes[MetaAttribute.Artist];
                SuppArtist.Text = finalAttributes[MetaAttribute.SupportingArtist] == "-1" ? "Various Artists" : finalAttributes[MetaAttribute.SupportingArtist];
                Genre.Text = finalAttributes[MetaAttribute.Genre] == "-1" ? "Various Genres" : finalAttributes[MetaAttribute.Genre];
                Track.Text = finalAttributes[MetaAttribute.TrackNumber] == "-1" ? "--" : finalAttributes[MetaAttribute.TrackNumber];
                Year.Text = finalAttributes[MetaAttribute.Year] == "-1" ? "----" : finalAttributes[MetaAttribute.Year];
                Rating.Text = finalAttributes[MetaAttribute.Rating] == "-1" ? "Various Ratings" : finalAttributes[MetaAttribute.Rating];
            }
        }

        private static string GetValue(MetaAttribute attribute, File song)
        {
            switch (attribute)
            {
                case MetaAttribute.SongTitle:
                    return song.GetSongTitle();
                case MetaAttribute.Album:
                    return song.GetAlbum();
                case MetaAttribute.Artist:
                    return song.GetArtist();
                case MetaAttribute.SupportingArtist:
                    return song.GetSupportingArtist();
                case MetaAttribute.Genre:
                    return song.GetGenre();
                case MetaAttribute.TrackNumber:
                    return song.GetTrack();
                case MetaAttribute.Year:
                    return song.GetYear();
                case MetaAttribute.Rating:
                    return song.GetRating();
                case MetaAttribute.AlbumArt:
                    return "-1";
                default:
                    return null;
            }
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

            EditCancelButton.Content = "Edit";

        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {

            if (EditCancelButton.Content.Equals("Edit") && SongTree.SelectedItems.Count > 0)
            {
                EditCancelButton.Content = "Cancel";
                ToggleReadOnlyFields(false);
            }
            else if (EditCancelButton.Content.Equals("Cancel"))
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
                EditCancelButton.Content = "Edit";
            }
        }

        private void FormCheck()
        {
            if (EditCancelButton.Content.Equals("Cancel"))
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
                EditCancelButton.Content = "Edit";
                PopulateMetadataForm();
            }
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            if (SongTitle.IsReadOnly) return;
            EditCancelButton.Content = "Edit";
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
                            song.SetSongTitle(changedBox.Text);
                        }
                        else if (changedBox.Equals(Album))
                        {
                            song.SetAlbum(changedBox.Text);
                        }
                        else if (changedBox.Equals(Artist))
                        {
                            song.SetArtist(changedBox.Text);
                        }
                        else if (changedBox.Equals(Genre))
                        {
                            song.SetGenre(changedBox.Text);
                        }
                        else if (changedBox.Equals(Track))
                        {
                            song.SetTrack(changedBox.Text);
                        }
                        else if (changedBox.Equals(Year))
                        {
                            song.SetYear(changedBox.Text);
                        }
                        else if (changedBox.Equals(Rating))
                        {
                            song.SetRating(changedBox.Text);
                        }
                        else if (changedBox.Equals(SuppArtist))
                        {
                            song.SetSupportingArtists(changedBox.Text);
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

        private void ToggleReadOnlyFields(Boolean toggle)
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
