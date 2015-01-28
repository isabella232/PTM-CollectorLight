using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;

namespace WpfApplication.CollectorLight.DomainModels
{
	// Helper class sto get a description string from HTML content
	internal sealed class HtmlToTextConverter : IValueConverter
	{
		private static string htmlLineBreakRegex = @"(<br */>)|(\[br */\])";
		//@"<br(.)*?>";	// Regular expression to strip HTML line break tag

		private static string htmlStripperRegex = @"<(.|\n)*?>"; // Regular expression to strip HTML tags

		public static readonly DependencyProperty HtmlToInlinesProperty =
			DependencyProperty.RegisterAttached("HtmlToInlines", typeof (string), typeof (HtmlToTextConverter),
				new PropertyMetadata(null, OnHtmlToInlinesPropertyChanged));

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string)
				return ToStrippedHtmlText(value);
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public static string GetHtmlToInlines(DependencyObject obj)
		{
			return (string) obj.GetValue(HtmlToInlinesProperty);
		}

		public static void SetHtmlToInlines(DependencyObject obj, string value)
		{
			obj.SetValue(HtmlToInlinesProperty, value);
		}

		// Using a DependencyProperty as the backing store for HtmlToInlinesProperty.  This enables animation, styling, binding, etc...

		private static void OnHtmlToInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is Paragraph)
			{
				if (e.NewValue == null)
					(d as Paragraph).Inlines.Clear();
				else
				{
					var splits = Regex.Split(e.NewValue as string, htmlLineBreakRegex,
						RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
					foreach (var line in splits)
					{
						string text = Regex.Replace(line, htmlStripperRegex, string.Empty);
						Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
						if (!string.IsNullOrWhiteSpace(text))
						{
							text = regex.Replace(text, @" "); //Remove multiple spaces
							text = text.Replace("&quot;", "\""); //Unencode quotes
							text = text.Replace("&nbsp;", " "); //Unencode spaces
							(d as Paragraph).Inlines.Add(new Run {Text = text});
							(d as Paragraph).Inlines.Add(new LineBreak());
						}
					}
				}
			}
		}

		private static string ToStrippedHtmlText(object input)
		{
			string retVal = string.Empty;

			if (input != null)
			{
				// Replace HTML line break tags with $LINEBREAK$:
				retVal = Regex.Replace(input as string, htmlLineBreakRegex, "", RegexOptions.IgnoreCase);
				// Remove the rest of HTML tags:
				retVal = Regex.Replace(retVal, htmlStripperRegex, string.Empty);
				//retVal.Replace("$LINEBREAK$", "\n");
				retVal = retVal.Trim();
			}

			return retVal;
		}
	}
}