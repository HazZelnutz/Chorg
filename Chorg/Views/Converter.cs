using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Chorg.Models;

namespace Chorg.Views
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ContentToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ContentType)value)
            {
                case ContentType.GENERAL:
                    return "GEN";
                case ContentType.TAXI:
                    return "TAXI";
                case ContentType.SID:
                    return "SID";
                case ContentType.STAR:
                    return "STAR";
                case ContentType.APP:
                    return "APP";
                case ContentType.UNDEFINED:
                    return "N.D.";
                default:
                    throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ContentToSolidColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ContentType)value)
            {
                case ContentType.GENERAL:
                    return (SolidColorBrush)App.Current.FindResource("Color_GEN");
                case ContentType.TAXI:
                    return (SolidColorBrush)App.Current.FindResource("Color_TAXI");
                case ContentType.SID:
                    return (SolidColorBrush)App.Current.FindResource("Color_SID");
                case ContentType.STAR:
                    return (SolidColorBrush)App.Current.FindResource("Color_STAR");
                case ContentType.APP:
                    return (SolidColorBrush)App.Current.FindResource("Color_APP");
                case ContentType.UNDEFINED:
                    return (SolidColorBrush)App.Current.FindResource("Color_ND");
                default:
                    throw new Exception();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
