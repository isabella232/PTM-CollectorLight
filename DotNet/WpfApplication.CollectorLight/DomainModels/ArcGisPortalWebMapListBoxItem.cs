using GalaSoft.MvvmLight.Command;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class ArcGisPortalWebMapListBoxItem
	{
		private readonly ArcGisPortalWebMapItem _arcGisPortalWebMapItem;
		private readonly RelayCommand<ArcGisPortalWebMapItem> _createOfflineMapCommand;
		private readonly RelayCommand<ArcGisPortalWebMapItem> _loadWebMapCommand;

		public ArcGisPortalWebMapListBoxItem(ArcGisPortalWebMapItem arcGisPortalWebMapItem,
			RelayCommand<ArcGisPortalWebMapItem> loadWebMapCommand, RelayCommand<ArcGisPortalWebMapItem> createOfflineMapCommand)
		{
			_arcGisPortalWebMapItem = arcGisPortalWebMapItem;
			_loadWebMapCommand = loadWebMapCommand;
			_createOfflineMapCommand = createOfflineMapCommand;
		}

		public ArcGisPortalWebMapItem ArcGisPortalWebMapItem
		{
			get { return _arcGisPortalWebMapItem; }
		}

		public RelayCommand<ArcGisPortalWebMapItem> LoadWebMapCommand
		{
			get { return _loadWebMapCommand; }
		}

		public RelayCommand<ArcGisPortalWebMapItem> CreateOfflineMapCommand
		{
			get { return _createOfflineMapCommand; }
		}
	}
}