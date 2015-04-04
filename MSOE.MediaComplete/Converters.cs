using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MSOE.MediaComplete.Lib.Playlists;

namespace MSOE.MediaComplete
{
    public class VolumeConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var nfi = new CultureInfo("en-US", false).NumberFormat;
            nfi.PercentDecimalDigits = 0;
            return (((double) value/100.0).ToString("P", nfi));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns Visibility based on whether an ObservableCollection of Playlists contains elements.
    /// </summary>
    public class PlaylistsEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var playlists = value as ObservableCollection<Playlist>;
            if (playlists == null)
            {
                return Visibility.Collapsed;
            }
            return playlists.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
