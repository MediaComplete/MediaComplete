﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
    /// Returns Visibility based on an integer argument (such as a collection count)
    /// </summary>
    public class IfZeroCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = value as int?;
            if (count == null)
            {
                return Visibility.Collapsed;
            }
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
