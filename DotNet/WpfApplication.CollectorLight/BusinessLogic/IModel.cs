using System;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using WpfApplication.CollectorLight.DomainModels;

namespace WpfApplication.CollectorLight.BusinessLogic
{
	public interface IModel
	{
		Uri DefaultServerUri { get; }
		bool IsSyncPossible { get; }
		event Action<OAuthTokenCredential> OAuth2TokenTokenReceived;
		event Action LoadLogInDialog;
		event Action<WebMap, ArcGISPortal> LoadWebMap;
		event Action<OfflineMapItem> LoadOfflineMap;
		event Action<OfflineMapItem> DeleteOfflineMap;
		event Action<string> ChangeMessageInfo;
		event Action<ArcGisPortalWebMapItem, ArcGISPortal> CreateOfflineMap;
		event Action RefreshOfflineMapItems;
		event Action<View, View, bool> ChangeView;
		event Action<RibbonTabState> ChangeRibbonTabSelectedState;
		event Action<string> ChangeUserNameInfo;
		event Action<string> ChangeMapNameInfo;
		event Action<LoadedMapType> ChangeMapTypeInfo;
		event Action ResetMapView;
		event Action ResetArcGisPortalItemsView;
		event Action<bool> NetworkStatusAvailabilityChanged;
		event Action<OfflineMapItem> SyncOfflineMap;
		event Action SyncStateChanged;
		event Action<string, ProgressStateItem> ProgressStateChanged;


		void TriggerOAuth2TokenTokenReceivedEvent(OAuthTokenCredential oAuth2Token);
		void TriggerLoadLogInDialogEvent();
		void TriggerLoadWebMapEvent(WebMap webMap, ArcGISPortal arcGisPortal);
		void TriggerLoadOfflineMapEvent(OfflineMapItem offlineMapItem);
		void TriggerDeleteOfflineMapEvent(OfflineMapItem offlineMapItem);
		void SetMessageInfo(string messageInfo, [CallerMemberName] string memberName = "");
		void TriggerCreateOfflineMapEvent(ArcGisPortalWebMapItem arcGisPortalWebMapItem, ArcGISPortal arcGisPortal);
		void TriggerRefreshOfflineMapItemsEvent();
		void TriggerSyncOfflineMapEvent(OfflineMapItem offlineMapItem);
		void TriggerProgressChangedEvent(string pathToOfflineMap, ProgressStateItem progressStateItem);
		void TriggerChangeViewEvent(View newView, View sourceView, bool changeBackToSource);
		void SetRibbonTabSelectedState(RibbonTabState ribbonTabState);
		void SetUserNameInfo(string userNameInfo);
		void SetLoadedMapNameInfo(string loadedMapNameInfo);
		void SetLoadedMapTypeInfo(LoadedMapType loadedMapTypeInfo);
		void DoResetMapView();
		void DoResetArcGisPortalItemsView();
		bool CheckIsNetworkAvailable();
		void SetSyncState(bool syncsAvailable);
		Uri GetOfflineMapItemsPath();
	}
}