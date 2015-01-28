using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Esri.ArcGISRuntime.Security;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;
using WpfApplication.CollectorLight.Views;

namespace WpfApplication.CollectorLight.ViewModels
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// You can also use Blend to data bind with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{
		private const string DEFAULT_SERVER_URL = @"https://www.arcgis.com/sharing/rest";
		private const string DEFAULT_NO_USERNAME_INFO = "No one is logged in";
		private const string DEFAULT_NO_MAPNAME_INFO = "No map is loaded";
		private const string DEFAULT_LOGGED_IN_INFO = " is logged in";
		private const string DEFAULT_LOGGED_OUT_INFO = " is logged out";
		private readonly IModel _model;
		private readonly StartScreenView _startScreenView;

		private ArcGisPortalWebMapItemsView _arcGisPortalWebMapItemsView;
		private bool _changeBackToPredecessorView;
		private CreateOfflineMapView _createOfflineMapView;
		private UserControl _currentUserControl;
		private bool _isArcGisPortalWebMapItemsSearchPossible;
		private bool _isMessageViewerVisible;
		private bool _isNetworkAvailable;
		private bool _isSyncPossible;
		private bool _isUserLoggedIn;
		private string _loadedMapNameInfo;
		private LoadedMapType _loadedMapType;
		private string _loadedMapTypeInfo;
		private LogInDialogView _logInDialogView;
		private MapView _mapView;
		private string _messageInfo;
		private OfflineMapItemsView _offlineMapItemsView;
		private View _predecessorView = View.StartScreenView;
		private int _ribbonTabSelectedIndex;
		private string _userNameInfo;

		public MainViewModel(IModel model)
		{
			_model = model;

			InitializeIdentityManager();
			InitializeModelEvents();
			InitializeViews();
			InitializeRelayCommands();
			InitializeAppIdHandlerEvents();
			IsUserLoggedIn = false;

			_startScreenView = new StartScreenView();

			_model.TriggerChangeViewEvent(View.StartScreenView, View.MainView, false);
			_model.SetUserNameInfo(DEFAULT_NO_USERNAME_INFO);
			_model.SetLoadedMapNameInfo(DEFAULT_NO_MAPNAME_INFO);
			_loadedMapType = LoadedMapType.None;

			IsNetworkAvailable = _model.CheckIsNetworkAvailable();
			IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible;
			IsArcGisPortalWebMapItemsSearchPossible = IsNetworkAvailable && IsUserLoggedIn;
			Messages = new ObservableCollection<string>();
		}

		public RelayCommand RibbonButtonAuthorizeLogInCommand { get; private set; }
		public RelayCommand RibbonButtonAuthorizeLogOutCommand { get; private set; }
		public RelayCommand RibbonButtonPortalItemsCommand { get; private set; }
		public RelayCommand RibbonSelectionChangedCommand { get; private set; }
		public RelayCommand RibbonButtonOfflineMapItemsCommand { get; private set; }
		public RelayCommand<RoutedEventArgs> RibbonLoadedCommand { get; private set; }
		public RelayCommand RibbonButtonDeleteFeaturesCommand { get; private set; }
		public RelayCommand RibbonButtonIdCommand { get; private set; }

		public RelayCommand MessageViewerCommand { get; private set; }
		public RelayCommand MessageViewerClearCommand { get; private set; }
		public ObservableCollection<string> Messages { get; set; }

		public bool IsMessageViewerVisible
		{
			get { return _isMessageViewerVisible; }
			set
			{
				_isMessageViewerVisible = value;
				RaisePropertyChanged("IsMessageViewerVisible");
			}
		}

		public bool IsNetworkAvailable
		{
			get { return _isNetworkAvailable; }
			set
			{
				_isNetworkAvailable = value;
				RaisePropertyChanged("IsNetworkAvailable");
			}
		}

		public bool IsUserLoggedIn
		{
			get { return _isUserLoggedIn; }
			set
			{
				_isUserLoggedIn = value;
				RaisePropertyChanged("IsUserLoggedIn");
			}
		}

		public bool IsSyncPossible
		{
			get { return _isSyncPossible; }
			set
			{
				_isSyncPossible = value;
				RaisePropertyChanged("IsSyncPossible");
			}
		}

		public bool IsArcGisPortalWebMapItemsSearchPossible
		{
			get { return _isArcGisPortalWebMapItemsSearchPossible; }
			set
			{
				_isArcGisPortalWebMapItemsSearchPossible = value;
				RaisePropertyChanged("IsArcGisPortalWebMapItemsSearchPossible");
			}
		}

		public int RibbonTabSelectedIndex
		{
			get { return _ribbonTabSelectedIndex; }
			set
			{
				_ribbonTabSelectedIndex = value;
				RaisePropertyChanged("RibbonTabSelectedIndex");
			}
		}

		public UserControl CurrentUserControl
		{
			get { return _currentUserControl; }
			set
			{
				_currentUserControl = value;
				RaisePropertyChanged("CurrentUserControl");
			}
		}

		public string MessageInfo
		{
			get { return _messageInfo; }
			set
			{
				_messageInfo = value;
				RaisePropertyChanged("MessageInfo");
			}
		}

		public string UserNameInfo
		{
			get { return _userNameInfo; }
			set
			{
				_userNameInfo = value;
				RaisePropertyChanged("UserNameInfo");
			}
		}

		public string LoadedMapNameInfo
		{
			get { return _loadedMapNameInfo; }
			set
			{
				_loadedMapNameInfo = value;
				RaisePropertyChanged("LoadedMapNameInfo");
			}
		}

		public string LoadedMapTypeInfo
		{
			get { return _loadedMapTypeInfo; }
			set
			{
				_loadedMapTypeInfo = value;
				RaisePropertyChanged("LoadedMapTypeInfo");
			}
		}

		private void InitializeAppIdHandlerEvents()
		{
			AppIdHandler.AgolAppIdChanged += () =>
			{
				_model.TriggerLoadLogInDialogEvent();
				if (IsUserLoggedIn)
				{
					ResetLogInCredentials();
					_model.TriggerChangeViewEvent(View.StartScreenView, View.MainView, false);
				}
			};
		}

		private void InitializeModelEvents()
		{
			_model.OAuth2TokenTokenReceived += p =>
			{
				if (p == null)
				{
					IsUserLoggedIn = false;
					IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible;
					IsArcGisPortalWebMapItemsSearchPossible = IsNetworkAvailable && IsUserLoggedIn;
					return;
				}
				IsUserLoggedIn = true;
				IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible;
				IsArcGisPortalWebMapItemsSearchPossible = IsNetworkAvailable && IsUserLoggedIn;
				UpdateIdentityManager(p);
				_model.DoResetArcGisPortalItemsView();
				_model.SetUserNameInfo(p.UserName);
				_model.SetMessageInfo(UserNameInfo + DEFAULT_LOGGED_IN_INFO);
			};

			_model.ChangeMessageInfo += p =>
			{
				if (p != string.Empty)
				{
					Messages.Insert(0, p);
				}

				MessageInfo = p;
			};

			_model.ChangeUserNameInfo += p => { UserNameInfo = p; };

			_model.ChangeMapNameInfo += p => { LoadedMapNameInfo = p; };

			_model.ChangeMapTypeInfo += p =>
			{
				_loadedMapType = p;

				switch (p)
				{
					case LoadedMapType.Online:
						LoadedMapTypeInfo = "[online map]";
						break;
					case LoadedMapType.Offline:
						LoadedMapTypeInfo = "[offline map]";
						break;
					case LoadedMapType.None:
						LoadedMapTypeInfo = string.Empty;
						break;
				}
			};

			_model.ChangeView += (newView, sourceView, changeBackToSource) =>
			{
				if (_changeBackToPredecessorView)
				{
					SetView(_predecessorView);
					_predecessorView = View.MainView;
					_changeBackToPredecessorView = false;
					return;
				}

				_predecessorView = sourceView;
				_changeBackToPredecessorView = changeBackToSource;
				SetView(newView);
			};

			_model.ChangeRibbonTabSelectedState += p =>
			{
				switch (p)
				{
					case RibbonTabState.Config:
						RibbonTabSelectedIndex = 0;
						break;
					case RibbonTabState.Map:
						RibbonTabSelectedIndex = 1;
						break;
				}
			};

			_model.NetworkStatusAvailabilityChanged += p =>
			{
				IsNetworkAvailable = p;
				IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible;
				IsArcGisPortalWebMapItemsSearchPossible = IsNetworkAvailable && IsUserLoggedIn;
			};

			_model.SyncStateChanged += () => { IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible; };
		}

		private void SetView(View view)
		{
			switch (view)
			{
				case View.MapView:
					_model.SetRibbonTabSelectedState(RibbonTabState.Map);
					CurrentUserControl = _mapView;
					break;
				case View.ArcGisPortalWebMapItemsView:
					_model.SetRibbonTabSelectedState(RibbonTabState.Config);
					CurrentUserControl = _arcGisPortalWebMapItemsView;
					break;
				case View.CreateOfflineMapView:
					_model.SetRibbonTabSelectedState(RibbonTabState.Config);
					CurrentUserControl = _createOfflineMapView;
					break;
				case View.LogInDialogView:
					CurrentUserControl = _logInDialogView;
					break;
				case View.OfflineMapItemsView:
					_model.SetRibbonTabSelectedState(RibbonTabState.Config);
					CurrentUserControl = _offlineMapItemsView;
					break;
				case View.StartScreenView:
					_model.SetRibbonTabSelectedState(RibbonTabState.Config);
					CurrentUserControl = _startScreenView;
					break;
			}
		}

		private void InitializeRelayCommands()
		{
			RibbonButtonAuthorizeLogInCommand = new RelayCommand(() =>
			{
				if (CurrentUserControl.Equals(_startScreenView))
				{
					_model.TriggerChangeViewEvent(View.LogInDialogView, View.StartScreenView, false);
				}
				else if (CurrentUserControl.Equals(_mapView))
				{
					_model.TriggerChangeViewEvent(View.LogInDialogView, View.MapView, true);
				}
				else if (CurrentUserControl.Equals(_arcGisPortalWebMapItemsView))
				{
					_model.TriggerChangeViewEvent(View.LogInDialogView, View.ArcGisPortalWebMapItemsView, true);
				}
				else if (CurrentUserControl.Equals(_offlineMapItemsView))
				{
					_model.TriggerChangeViewEvent(View.LogInDialogView, View.OfflineMapItemsView, true);
				}

				_model.TriggerLoadLogInDialogEvent();
			});

			RibbonButtonAuthorizeLogOutCommand = new RelayCommand(ResetLogInCredentials);

			RibbonButtonPortalItemsCommand =
				new RelayCommand(() => _model.TriggerChangeViewEvent(View.ArcGisPortalWebMapItemsView, View.MainView, false));

			RibbonSelectionChangedCommand = new RelayCommand(() =>
			{
				if (RibbonTabSelectedIndex == 0)
				{
					if (IsUserLoggedIn)
					{
						if (_loadedMapType == LoadedMapType.Online)
						{
							_model.TriggerChangeViewEvent(View.ArcGisPortalWebMapItemsView, View.MapView, false);
						}
						else if (_loadedMapType == LoadedMapType.Offline)
						{
							_model.TriggerChangeViewEvent(View.OfflineMapItemsView, View.MapView, false);
						}
					}
					else
					{
						_model.TriggerChangeViewEvent(View.OfflineMapItemsView, View.MapView, false);
					}
				}
				if (RibbonTabSelectedIndex == 1)
				{
					_model.TriggerChangeViewEvent(View.MapView, View.MainView, false);
				}
			});

			RibbonButtonOfflineMapItemsCommand =
				new RelayCommand(() => _model.TriggerChangeViewEvent(View.OfflineMapItemsView, View.MainView, false));

			RibbonLoadedCommand = new RelayCommand<RoutedEventArgs>(p =>
			{
				//Entfernt die Ribbon QuickAccessToolBar
				var child = VisualTreeHelper.GetChild((DependencyObject) p.Source, 0) as Grid;
				if (child != null)
				{
					child.RowDefinitions[0].Height = new GridLength(0);
				}
			});

			RibbonButtonIdCommand = new RelayCommand(() => AppIdHandler.ChangeAgolAppId());

			MessageViewerCommand = new RelayCommand(() => { IsMessageViewerVisible = !IsMessageViewerVisible; });

			MessageViewerClearCommand = new RelayCommand(() => Messages.Clear());
		}

		private void ResetLogInCredentials()
		{
			_model.SetMessageInfo(UserNameInfo + DEFAULT_LOGGED_OUT_INFO);
			RemoveCredentialFromIdentityManager();
			IsUserLoggedIn = false;
			IsSyncPossible = IsNetworkAvailable && IsUserLoggedIn && _model.IsSyncPossible;
			IsArcGisPortalWebMapItemsSearchPossible = IsNetworkAvailable && IsUserLoggedIn;
			_model.DoResetArcGisPortalItemsView();
			_model.DoResetMapView();
			_model.SetUserNameInfo(DEFAULT_NO_USERNAME_INFO);
			_model.SetLoadedMapNameInfo(DEFAULT_NO_MAPNAME_INFO);
		}

		private void RemoveCredentialFromIdentityManager()
		{
			var credential = IdentityManager.Current.FindCredential(DEFAULT_SERVER_URL);
			IdentityManager.Current.RemoveCredential(credential);
		}

		private void InitializeViews()
		{
			_logInDialogView = new LogInDialogView();
			_arcGisPortalWebMapItemsView = new ArcGisPortalWebMapItemsView();
			_mapView = new MapView();
			_createOfflineMapView = new CreateOfflineMapView();
			_offlineMapItemsView = new OfflineMapItemsView();
		}

		private void UpdateIdentityManager(TokenCredential token)
		{
			var arcGisTokenCredential = new ArcGISTokenCredential {Token = token.Token, ServiceUri = DEFAULT_SERVER_URL};
			IdentityManager.Current.AddCredential(arcGisTokenCredential);
		}

		private void InitializeIdentityManager()
		{
			var challengeHandler = new ChallengeHandler(Challenge);
			IdentityManager.Current.ChallengeHandler = challengeHandler;
		}

		private Task<Credential> Challenge(CredentialRequestInfo arg)
		{
			return Task.FromResult<Credential>(null);
		}
	}
}