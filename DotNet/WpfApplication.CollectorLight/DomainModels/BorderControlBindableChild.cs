using System.Windows;
using System.Windows.Controls;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class BorderControlBindableChild
	{
		public static readonly DependencyProperty BindableChildProperty =
			DependencyProperty.RegisterAttached("BindableChild", typeof (UIElement), typeof (BorderControlBindableChild),
				new UIPropertyMetadata(null, BindableChildPropertyChanged));

		public static UIElement GetBindableChild(DependencyObject obj)
		{
			return (UIElement) obj.GetValue(BindableChildProperty);
		}

		public static void SetBindableChild(DependencyObject obj, UIElement value)
		{
			obj.SetValue(BindableChildProperty, value);
		}

		private static void BindableChildPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var ele = sender as Decorator;
			ele.Child = (UIElement) e.NewValue;
		}
	}
}