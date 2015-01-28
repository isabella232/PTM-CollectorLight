/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:WpfApplication.CollectorLight"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using WpfApplication.CollectorLight.BusinessLogic;

namespace WpfApplication.CollectorLight.ViewModels
{
	/// <summary>
	/// This class contains static references to all the view models in the
	/// application and provides an entry point for the bindings.
	/// </summary>
	public class ViewModelLocator
	{
		/// <summary>
		/// Initializes a new instance of the ViewModelLocator class.
		/// </summary>
		public ViewModelLocator()
		{
			ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

			SimpleIoc.Default.Register<IModel, Model>();
			var model = ServiceLocator.Current.GetInstance<IModel>();
			SimpleIoc.Default.Register(() => new MainViewModel(model));
			SimpleIoc.Default.Register(() => new LogInDialogViewModel(model));
			SimpleIoc.Default.Register(() => new ArcGisPortalWebMapItemsViewModel(model));
			SimpleIoc.Default.Register(() => new MapViewModel(model));
			SimpleIoc.Default.Register(() => new CreateOfflineMapViewModel(model));
			SimpleIoc.Default.Register(() => new OfflineMapItemsViewModel(model));
		}

		public MainViewModel MainVm
		{
			get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
		}

		public LogInDialogViewModel LogInDialogVm
		{
			get { return ServiceLocator.Current.GetInstance<LogInDialogViewModel>(); }
		}

		public ArcGisPortalWebMapItemsViewModel ArcGisPortalWebMapItemsVm
		{
			get { return ServiceLocator.Current.GetInstance<ArcGisPortalWebMapItemsViewModel>(); }
		}

		public MapViewModel MapVm
		{
			get { return ServiceLocator.Current.GetInstance<MapViewModel>(); }
		}

		public CreateOfflineMapViewModel CreateOfflineMapVm
		{
			get { return ServiceLocator.Current.GetInstance<CreateOfflineMapViewModel>(); }
		}

		public OfflineMapItemsViewModel OfflineMapItemsVm
		{
			get { return ServiceLocator.Current.GetInstance<OfflineMapItemsViewModel>(); }
		}
	}
}