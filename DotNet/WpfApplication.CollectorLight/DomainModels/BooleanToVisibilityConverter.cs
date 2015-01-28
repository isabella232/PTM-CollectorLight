using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else

#endif

namespace WpfApplication.CollectorLight.DomainModels
{
	/// <summary>
	/// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
	/// <see cref="Visibility.Collapsed"/>.
	/// </summary>
	internal sealed class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (value is bool && (bool) value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return value is Visibility && (Visibility) value == Visibility.Visible;
		}

#if !NETFX_CORE
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value, targetType, parameter, culture.ThreeLetterWindowsLanguageName);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
#endif
	}
}