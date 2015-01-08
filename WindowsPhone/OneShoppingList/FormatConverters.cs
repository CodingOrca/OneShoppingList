using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using OneShoppingList.Resources;
using OneShoppingList.ViewModel;
using OneShoppingList.Model;
using Microsoft.Phone.Controls;
using LinqToVisualTree;
using System.Linq;

namespace OneShoppingList
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            return b ? AppResources.on : AppResources.off;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string p = parameter as string;
            if (p != null)
            {
                return String.Format(culture, p, value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString().ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class ToVisibilityConverter : IValueConverter 
    { 
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        { 
            return System.Convert.ToBoolean(value) ? Visibility.Visible : Visibility.Collapsed; 
        } 
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        { 
            return value.Equals(Visibility.Visible); 
        } 
    }

    public class NToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) == System.Convert.ToInt32(parameter) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible);
        }
    }

    public class ToVisibilityInvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value) ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Collapsed);
        }
    }

    public class BooleanToTextDecorationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            return b ? TextDecorations.Underline : null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(TextDecorations.Underline);
        }
    }

    public class BooleanToStaticResourceConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            string p = parameter as string;
            if (p != null)
            {
                string[] parts = p.Split(';');
                return Application.Current.Resources[b ? parts[0] : parts[1]];
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class BoolNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            return !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = System.Convert.ToBoolean(value);
            return !b;
        }
    }

    public class ShopToBrushConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush foregroundBrush = App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            SolidColorBrush accentBrush = App.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            Shop CurrentShop = (App.Current.Resources["Locator"] as ViewModelLocator).Main.CurrentShop;
            string shop = value as string;
            if (shop == null || CurrentShop == null) return foregroundBrush;

            bool isPrimaryShop = shop == CurrentShop.Name;
            return isPrimaryShop ? foregroundBrush : accentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToFullOrHalfOpacityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return System.Convert.ToBoolean(value) ? 1.0 : 0.5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToActiveBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush accentBrush = App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            SolidColorBrush transparentBrush = App.Current.Resources["PhoneChromeBrush"] as SolidColorBrush;
            bool criteria = System.Convert.ToBoolean(value);
            return criteria ? accentBrush : transparentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ToChromeBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush chromeBrush = App.Current.Resources["PhoneChromeBrush"] as SolidColorBrush;
            SolidColorBrush backgroundBrush = App.Current.Resources["PhoneForBrush"] as SolidColorBrush;
            bool criteria = System.Convert.ToBoolean(value);
            return criteria ? chromeBrush : backgroundBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolToDisabledBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush accentBrush = App.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush;
            SolidColorBrush transparentBrush = App.Current.Resources["PhoneDisabledBrush"] as SolidColorBrush;
            bool criteria = System.Convert.ToBoolean(value);
            return criteria ? transparentBrush : accentBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LargerThenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int leftNumber = System.Convert.ToInt32(value);
            int rightNumber = System.Convert.ToInt32(parameter);
            return leftNumber > rightNumber;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class LessThenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int leftNumber = System.Convert.ToInt32(value);
            int rightNumber = System.Convert.ToInt32(parameter);
            return leftNumber < rightNumber;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
