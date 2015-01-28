using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;
using WpfApplication.CollectorLight.DomainModels.Json;

namespace WpfApplication.CollectorLight.ViewModels
{
	public class OfflineMapItemsViewModel : ViewModelBase
	{
		private const string PortalItemJsonFileName = "portalitem.json";

		private readonly IModel _model;

		private ObservableCollection<OfflineMapListBoxItem> _offlineMapListBoxItems;

		public OfflineMapItemsViewModel(IModel model)
		{
			_model = model;
			InitializeModelEvents();
			InitializeRelayCommands();
			OfflineMapListBoxItems = CreateOfflineMapItems();
		}

		public ObservableCollection<OfflineMapListBoxItem> OfflineMapListBoxItems
		{
			get { return _offlineMapListBoxItems; }
			set
			{
				_offlineMapListBoxItems = value;
				RaisePropertyChanged("OfflineMapListBoxItems");
			}
		}

		public RelayCommand<OfflineMapItem> LoadOfflineMapCommand { get; private set; }
		public RelayCommand<OfflineMapItem> SyncOfflineMapCommand { get; private set; }
		public RelayCommand<OfflineMapItem> DeleteOfflineMapCommand { get; private set; }

		private void InitializeRelayCommands()
		{
			LoadOfflineMapCommand = new RelayCommand<OfflineMapItem>(p =>
			{
				_model.TriggerLoadOfflineMapEvent(p);
				_model.SetLoadedMapNameInfo(p.OfflineMapName);
				_model.SetLoadedMapTypeInfo(LoadedMapType.Offline);
				_model.TriggerChangeViewEvent(View.MapView, View.OfflineMapItemsView, false);
			});

			SyncOfflineMapCommand = new RelayCommand<OfflineMapItem>(p => _model.TriggerSyncOfflineMapEvent(p), p => _model.IsSyncPossible);

			DeleteOfflineMapCommand = new RelayCommand<OfflineMapItem>(p =>
			{
				var listboxItem = OfflineMapListBoxItems.Single(item => item.OfflineMapItem == p);
				OfflineMapListBoxItems.Remove(listboxItem);
				_model.TriggerDeleteOfflineMapEvent(p);
			});
		}

		private void InitializeModelEvents()
		{
			_model.RefreshOfflineMapItems += () => { OfflineMapListBoxItems = CreateOfflineMapItems(); };

			_model.ProgressStateChanged += (p, q) =>
			{
				foreach (var item in OfflineMapListBoxItems)
				{
					if (item.OfflineMapItem.OfflineMapPath.Equals(p))
					{
						item.ProgressStateItem = q;
					}
				}
			};
		}

		private ObservableCollection<OfflineMapListBoxItem> CreateOfflineMapItems()
		{
			var offlineMapListBoxItems = new ObservableCollection<OfflineMapListBoxItem>();
			var webMapdirectories = GetWebMapdirectories();

			foreach (var webMapDirectory in webMapdirectories)
			{
				var portalItemJson = GetPortalItemJson(webMapDirectory);
				if (portalItemJson == null)
				{
					continue;
				}

				var directories = Directory.GetDirectories(webMapDirectory);
				foreach (var directory in directories)
				{
					var offlineMapName = directory.Split('\\').Last();
					var offlineMapItem = new OfflineMapItem(portalItemJson, offlineMapName, Path.GetFullPath(directory));

					var offlineMapListBoxItem = new OfflineMapListBoxItem(offlineMapItem, LoadOfflineMapCommand, SyncOfflineMapCommand,
						DeleteOfflineMapCommand);
					offlineMapListBoxItems.Add(offlineMapListBoxItem);
				}
			}

			return offlineMapListBoxItems;
		}

		private PortalItemJson GetPortalItemJson(string webMapPath)
		{
			try
			{
				using (var readfile = new StreamReader(Path.Combine(webMapPath, PortalItemJsonFileName)))
				{
					var jsonString = readfile.ReadToEnd();
					return new PortalItemJson(jsonString);
				}
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		private IEnumerable<string> GetWebMapdirectories()
		{
			var offlineMapItemPath = Path.GetFullPath(_model.GetOfflineMapItemsPath().AbsolutePath);
			if (!Directory.Exists(offlineMapItemPath))
			{
				return new string[0];
			}
			return Directory.GetDirectories(offlineMapItemPath);
		}
	}
}