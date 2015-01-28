using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Navigation;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;

namespace WpfApplication.CollectorLight.ViewModels
{
	public class LogInDialogViewModel : ViewModelBase
	{
		//private const string AGOL_APPID = "Ont2UoBOr9qfgcyg";

		private const string MessageAuthorizationCodeReceived = "AuthorizationCode received";
		private const string MessageOAuth2TokenReceived = "OAuth2Token received";
		private readonly IModel _model;

		private readonly OAuth2Client _oAuth2Client;
		private Uri _webBrowserSource;

		public LogInDialogViewModel(IModel model)
		{
			_model = model;
			_oAuth2Client = new OAuth2Client();
			InitializeModelEvents();
			InitializeRelayCommands();
		}

		public RelayCommand<NavigationEventArgs> AuthorizationWebResponseCommand { get; private set; }

		public Uri WebBrowserSource
		{
			get { return _webBrowserSource; }
			set
			{
				_webBrowserSource = value;
				RaisePropertyChanged("WebBrowserSource");
			}
		}

		private void InitializeModelEvents()
		{
			_model.LoadLogInDialog += SetWebBrowserSource;
		}

		private void SetWebBrowserSource()
		{
			//erst null definieren ist notwendig, ansonsten wird die URI nicht korrekt geladen
			WebBrowserSource = null;
			WebBrowserSource = _oAuth2Client.GetLogInDialogUri();
		}

		private void InitializeRelayCommands()
		{
			AuthorizationWebResponseCommand = new RelayCommand<NavigationEventArgs>(async args =>
			{
				var uri = args.Uri;
				if (string.IsNullOrEmpty(uri.Query))
					return;

				var nameValueCollection = HttpUtility.ParseQueryString(uri.Query);

				if (nameValueCollection.AllKeys.Contains("code"))
				{
					_model.TriggerChangeViewEvent(View.ArcGisPortalWebMapItemsView, View.LogInDialogView, false);
					_model.SetMessageInfo(MessageAuthorizationCodeReceived);
					var authorizationCode = nameValueCollection.GetValues("code")[0];
					await GetToken(authorizationCode);
				}
			}
				);
		}

		private async Task GetToken(string authorizationCode)
		{
			try
			{
				var token = await _oAuth2Client.GetToken(authorizationCode);

				_model.SetMessageInfo(MessageOAuth2TokenReceived);
				_model.TriggerOAuth2TokenTokenReceivedEvent(token);
			}
			catch (AuthenticationException ex)
			{
				_model.TriggerChangeViewEvent(View.LogInDialogView, View.ArcGisPortalWebMapItemsView, false);
				// Wir wollen hier explizit nicht warten, ist nur eine Hinweisbox.
				// ReSharper disable once CSharpWarnings::CS4014
				Task.Run(() => MessageBox.Show(ex.ToString()));
			}

			SetWebBrowserSource();
		}
	}
}