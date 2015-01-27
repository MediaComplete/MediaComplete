using System;
using System.Collections.Generic;
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
                var attributes = new Dictionary<MetaAttribute, string>
                {
                    {MetaAttribute.SongTitle, null},
                    {MetaAttribute.Album, null},
                    {MetaAttribute.Artist, null},
                    {MetaAttribute.SupportingArtist, null},
                    {MetaAttribute.Genre, null},
                    {MetaAttribute.TrackNumber, null},
                    {MetaAttribute.Year, null},
                    {MetaAttribute.Rating, null}
                };
                foreach ( SongTreeViewItem item in SongTree.SelectedItems)
                {
                    try
                    {
                        var song = File.Create(item.GetPath());

                        switch (attributes[MetaAttribute.SongTitle])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.SongTitle] = song.GetSongTitle();
                                break;
                            default:
                                if (attributes[MetaAttribute.SongTitle] != song.GetSongTitle())
                                {
                                    attributes[MetaAttribute.SongTitle] = "-1";
                                }
                                break;
                        }
                        switch (attributes[MetaAttribute.Album])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.Album] = song.GetAlbum();
                                break;
                            default:
                                if (attributes[MetaAttribute.Album] != song.GetAlbum())
                                {
                                    attributes[MetaAttribute.Album] = "-1";
                                }
                                break;
                        }

                        switch (attributes[MetaAttribute.Artist])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.Artist] = song.GetArtist();
                                break;
                            default:
                                if (attributes[MetaAttribute.Artist] != song.GetArtist())
                                {
                                    attributes[MetaAttribute.Artist] = "-1";
                                }
                                break;
                        }
                        switch (attributes[MetaAttribute.SupportingArtist])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.SupportingArtist] = song.GetSupportingArtist();
                                break;
                            default:
                                if (attributes[MetaAttribute.SupportingArtist] != song.GetSupportingArtist())
                                {
                                    attributes[MetaAttribute.SupportingArtist] = "-1";
                                }
                                break;
                        }

                        switch (attributes[MetaAttribute.Genre])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.Genre] = song.GetGenre();
                                break;
                            default:
                                if (attributes[MetaAttribute.Genre] != song.GetGenre())
                                {
                                    attributes[MetaAttribute.Genre] = "-1";
                                }
                                break;
                        }

                        switch (attributes[MetaAttribute.TrackNumber])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.TrackNumber] = song.GetTrack();
                                break;
                            default:
                                if (attributes[MetaAttribute.TrackNumber] != song.GetTrack())
                                {
                                    attributes[MetaAttribute.TrackNumber] = "-1";
                                }
                                break;
                        }

                        switch (attributes[MetaAttribute.Year])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.Year] = song.GetYear();
                                break;
                            default:
                                if (attributes[MetaAttribute.Year] != song.GetYear())
                                {
                                    attributes[MetaAttribute.Year] = "-1";
                                }
                                break;
                        }

                        switch (attributes[MetaAttribute.Rating])
                        {
                            case "-1":
                                break;
                            case null:
                                attributes[MetaAttribute.Rating] = song.GetRating();
                                break;
                            default:
                                if (attributes[MetaAttribute.Rating] != song.GetRating())
                                {
                                    attributes[MetaAttribute.Rating] = "-1";
                                }
                                break;
                        }
                    }
                    catch (CorruptFileException)
                    {
                        StatusBarHandler.Instance.ChangeStatusBarMessage("CorruptFile-Error", StatusBarHandler.StatusIcon.Error);
                    }
                }
                SongTitle.Text = attributes[MetaAttribute.SongTitle] == "-1" ? "Various Songs" : attributes[MetaAttribute.SongTitle];
                Album.Text = attributes[MetaAttribute.Album] == "-1" ? "Various Albums" : attributes[MetaAttribute.Album];
                Artist.Text = attributes[MetaAttribute.Artist] == "-1" ? "Various Artists" : attributes[MetaAttribute.Artist];
                SuppArtist.Text = attributes[MetaAttribute.SupportingArtist] == "-1" ? "Various Artists" : attributes[MetaAttribute.SupportingArtist];
                Genre.Text = attributes[MetaAttribute.Genre] == "-1" ? "Various Genres" : attributes[MetaAttribute.Genre];
                Track.Text = attributes[MetaAttribute.TrackNumber] == "-1" ? "--" : attributes[MetaAttribute.TrackNumber];
                Year.Text = attributes[MetaAttribute.Year] == "-1" ? "----" : attributes[MetaAttribute.Year];
                Rating.Text = attributes[MetaAttribute.Rating] == "-1" ? "Various Ratings" : attributes[MetaAttribute.Rating];
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
                _changedBoxes.Clear();
                ToggleReadOnlyFields(true);
                EditCancelButton.Content = "Edit";
                PopulateMetadataForm();
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
