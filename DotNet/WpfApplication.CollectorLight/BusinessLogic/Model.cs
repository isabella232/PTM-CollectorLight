using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using WpfApplication.CollectorLight.DomainModels;

namespace WpfApplication.CollectorLight.BusinessLogic
{
	public class Model : IModel
	{
		private readonly Uri _defaultServerUri = new Uri(@"https://www.arcgis.com/sharing/rest");
		private readonly TimeSpan _messageTimespan = TimeSpan.FromSeconds(10);
		private readonly Uri _offlineMapsPath = new Uri(Environment.CurrentDirectory + "/OfflineMaps");
		private DispatcherTimer _messageTimer;

		public Model()
		{
			NetworkStatus.AvailabilityChanged += (p, q) => NetworkStatusAvailabilityChanged(q.IsAvailable);
			SetMessageTimer(_messageTimespan);
		}

		public event Action<OAuthTokenCredential> OAuth2TokenTokenReceived = delegate { };
		public event Action LoadLogInDialog = delegate { };
		public event Action<WebMap, ArcGISPortal> LoadWebMap = delegate { };
		public event Action<OfflineMapItem> LoadOfflineMap = delegate { };
		public event Action<OfflineMapItem> DeleteOfflineMap = delegate { };
		public event Action<string> ChangeMessageInfo = delegate { };
		public event Action<ArcGisPortalWebMapItem, ArcGISPortal> CreateOfflineMap = delegate { };
		public event Action RefreshOfflineMapItems = delegate { };
		public event Action<View, View, bool> ChangeView = delegate { };
		public event Action<RibbonTabState> ChangeRibbonTabSelectedState = delegate { };
		public event Action<string> ChangeUserNameInfo = delegate { };
		public event Action<string> ChangeMapNameInfo = delegate { };
		public event Action<LoadedMapType> ChangeMapTypeInfo = delegate { };
		public event Action ResetMapView = delegate { };
		public event Action ResetArcGisPortalItemsView = delegate { };
		public event Action<bool> NetworkStatusAvailabilityChanged = delegate { };
		public event Action<OfflineMapItem> SyncOfflineMap = delegate { };
		public event Action SyncStateChanged = delegate { };
		public event Action<string, ProgressStateItem> ProgressStateChanged = delegate { };

		public Uri DefaultServerUri
		{
			get { return _defaultServerUri; }
		}

		public bool IsSyncPossible { get; private set; }

		public void TriggerOAuth2TokenTokenReceivedEvent(OAuthTokenCredential oAuth2Token)
		{
			OAuth2TokenTokenReceived(oAuth2Token);
		}

		public void TriggerLoadLogInDialogEvent()
		{
			LoadLogInDialog();
		}

		public void TriggerLoadWebMapEvent(WebMap webMap, ArcGISPortal arcGisPortal)
		{
			LoadWebMap(webMap, arcGisPortal);
		}

		public void TriggerLoadOfflineMapEvent(OfflineMapItem offlineMapItem)
		{
			LoadOfflineMap(offlineMapItem);
		}

		public void TriggerDeleteOfflineMapEvent(OfflineMapItem offlineMapItem)
		{
			DeleteOfflineMap(offlineMapItem);
		}

		public void SetMessageInfo(string message, [CallerMemberName] string memberName = "")
		{
			if (message == string.Empty)
				return;

			_messageTimer.Start();

			message = string.Format("[{0}] ({1}) {2}", DateTime.Now.ToLongTimeString(), memberName, message);

			ChangeMessageInfo(message);
		}

		public void TriggerCreateOfflineMapEvent(ArcGisPortalWebMapItem arcGisPortalWebMapItem, ArcGISPortal arcGisPortal)
		{
			CreateOfflineMap(arcGisPortalWebMapItem, arcGisPortal);
		}

		public void TriggerRefreshOfflineMapItemsEvent()
		{
			RefreshOfflineMapItems();
		}

		public void TriggerSyncOfflineMapEvent(OfflineMapItem offlineMapItem)
		{
			SyncOfflineMap(offlineMapItem);
		}

		public void TriggerProgressChangedEvent(string pathToOfflineMap, ProgressStateItem progressStateItem)
		{
			ProgressStateChanged(pathToOfflineMap, progressStateItem);
		}

		public void TriggerChangeViewEvent(View newView, View sourceView, bool changeBackToSource)
		{
			ChangeView(newView, sourceView, changeBackToSource);
		}

		public void SetRibbonTabSelectedState(RibbonTabState ribbonTabState)
		{
			ChangeRibbonTabSelectedState(ribbonTabState);
		}

		public void SetUserNameInfo(string userNameInfo)
		{
			ChangeUserNameInfo(userNameInfo);
		}

		public void SetLoadedMapNameInfo(string loadedMapNameInfo)
		{
			ChangeMapNameInfo(loadedMapNameInfo);
		}

		public void SetLoadedMapTypeInfo(LoadedMapType loadedMapTypeInfo)
		{
			ChangeMapTypeInfo(loadedMapTypeInfo);
		}

		public void DoResetMapView()
		{
			ResetMapView();
		}

		public void DoResetArcGisPortalItemsView()
		{
			ResetArcGisPortalItemsView();
		}

		public bool CheckIsNetworkAvailable()
		{
			return NetworkStatus.IsAvailable;
		}

		public void SetSyncState(bool syncsAvailable)
		{
			IsSyncPossible = syncsAvailable;
			SyncStateChanged();
		}

		public Uri GetOfflineMapItemsPath()
		{
			return _offlineMapsPath;
		}

		private void SetMessageTimer(TimeSpan timeSpan)
		{
			_messageTimer = new DispatcherTimer(timeSpan, DispatcherPriority.Normal, delegate
			{
				_messageTimer.Stop();
				SetMessageInfo(string.Empty);
			}, Application.Current.Dispatcher);
		}
	}
}