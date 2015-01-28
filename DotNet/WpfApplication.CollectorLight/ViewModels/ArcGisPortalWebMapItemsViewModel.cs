using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;

namespace WpfApplication.CollectorLight.ViewModels
{
	public class ArcGisPortalWebMapItemsViewModel : ViewModelBase
	{
		private readonly IModel _model;

		private ArcGISPortal _arcGisPortal;
		private ObservableCollection<ArcGisPortalWebMapListBoxItem> _arcGisPortalWebMapListBoxItems;
		private bool _isBusy;
		private string _searchText = string.Empty;

		public ArcGisPortalWebMapItemsViewModel(IModel model)
		{
			_model = model;
			ArcGisPortalWebMapListBoxItems = new ObservableCollection<ArcGisPortalWebMapListBoxItem>();
			InitializModelEvents();
			InitializeCommands();
		}

		public string SearchText
		{
			get { return _searchText; }
			set
			{
				_searchText = value.Trim();
				RaisePropertyChanged("SearchText");
			}
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				_isBusy = value;
				RaisePropertyChanged("IsBusy");
			}
		}

		public ObservableCollection<ArcGisPortalWebMapListBoxItem> ArcGisPortalWebMapListBoxItems
		{
			get { return _arcGisPortalWebMapListBoxItems; }
			set
			{
				_arcGisPortalWebMapListBoxItems = value;
				RaisePropertyChanged("ArcGisPortalWebMapListBoxItems");
			}
		}

		public RelayCommand ArcGisPortalSearchCommand { get; private set; }
		public RelayCommand<ArcGisPortalWebMapItem> LoadWebMapCommand { get; private set; }
		public RelayCommand<ArcGisPortalWebMapItem> CreateOfflineMapCommand { get; private set; }

		private void InitializModelEvents()
		{
			_model.ResetArcGisPortalItemsView += Reset;
		}

		private void InitializeCommands()
		{
			ArcGisPortalSearchCommand = new RelayCommand(() =>
			{
				ArcGisPortalWebMapListBoxItems.Clear();
				SearchArcgisOnline();
			});

			LoadWebMapCommand = new RelayCommand<ArcGisPortalWebMapItem>(p =>
			{
				_model.TriggerLoadWebMapEvent(p.WebMap, _arcGisPortal);
				_model.TriggerChangeViewEvent(View.MapView, View.ArcGisPortalWebMapItemsView, false);
				_model.SetLoadedMapNameInfo(p.ArcGisPortalItem.Title);
				_model.SetLoadedMapTypeInfo(LoadedMapType.Online);
			});

			CreateOfflineMapCommand = new RelayCommand<ArcGisPortalWebMapItem>(p =>
			{
				_model.TriggerChangeViewEvent(View.CreateOfflineMapView, View.ArcGisPortalWebMapItemsView, false);
				_model.TriggerCreateOfflineMapEvent(p, _arcGisPortal);
			});
		}

		private async void SearchArcgisOnline()
		{
			try
			{
				IsBusy = true;

				//create Portal instance
				try
				{
					_arcGisPortal = await ArcGISPortal.CreateAsync(_model.DefaultServerUri);
				}
				catch (ArcGISWebException agwex)
				{
					_model.SetMessageInfo(agwex.Message);
					if ((int) agwex.Code == 498)
					{
						MessageBox.Show("Der Token ist abgelaufen");
					}
					return;
				}

				//define search creteria
				var sb = new StringBuilder();
				sb.Append(string.Format("{0} ", SearchText));
				sb.Append("type:(\"web map\" NOT \"web mapping application\") ");
				sb.Append("typekeywords:(\"offline\") ");

				if (_arcGisPortal.CurrentUser != null &&
				    _arcGisPortal.ArcGISPortalInfo != null &&
				    !string.IsNullOrEmpty(_arcGisPortal.ArcGISPortalInfo.Id))
				{
					sb.Append(string.Format("orgid:(\"{0}\")", _arcGisPortal.ArcGISPortalInfo.Id));
				}

				var searchParams = new SearchParameters(sb.ToString())
				{
					Limit = 20,
					SortField = "avgrating",
					SortOrder = QuerySortOrder.Descending,
				};

				//search for items
				var result = await _arcGisPortal.SearchItemsAsync(searchParams);

				//rate the result items
				foreach (var item in result.Results.Where(x => x.TypeKeywords.Contains("Offline")))
				{
					try
					{
						var webMap = await WebMap.FromPortalItemAsync(item);
						bool isEditable = false;
						bool isSyncable = false;

						if (webMap.OperationalLayers.Count > 0)
						{
							foreach (var operationalLayer in webMap.OperationalLayers)
							{
								if (!operationalLayer.Url.Contains("FeatureServer")) continue;

								var featureLayer = new FeatureLayer(new Uri(operationalLayer.Url));
								await featureLayer.InitializeAsync();
								var serviceFeatureTable = featureLayer.FeatureTable as ServiceFeatureTable;
								var capabilities = serviceFeatureTable.ServiceInfo.Capabilities;

								foreach (var capability in capabilities)
								{
									if (capability == "Editing")
									{
										isEditable = true;
									}
									if (capability == "Sync")
									{
										isSyncable = true;
									}
								}

								if (isEditable)
								{
									var arcGisPortalWebMapItem = new ArcGisPortalWebMapItem(item, webMap, isEditable, isSyncable);
									ArcGisPortalWebMapListBoxItems.Add(new ArcGisPortalWebMapListBoxItem(arcGisPortalWebMapItem, LoadWebMapCommand, CreateOfflineMapCommand));
									break;
								}
							}
						}
					}
					catch
					{
					}
				}
			}
			finally
			{
				IsBusy = false;
			}
		}

		private void Reset()
		{
			ArcGisPortalWebMapListBoxItems.Clear();
			SearchText = string.Empty;
		}
	}
}