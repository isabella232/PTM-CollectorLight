using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Security;

namespace WpfApplication.CollectorLight.DomainModels
{
	/// <summary>
	/// handles OAuth2 communication
	/// </summary>
	/// <seealso cref="https://developers.arcgis.com/authentication/user-ios-etc.html"/>
	public class OAuth2Client
	{
		public const string ArcGISOnlineOauthUri = @"https://www.arcgis.com/sharing/oauth2/authorize";
		public const string ArcGISOnlineRedirectUriQuery = "urn:ietf:wg:oauth:2.0:oob";
		public const string ArcGISOnlineTokenUri = "https://www.arcgis.com/sharing/oauth2/token";

		private static string GetAppId()
		{
			return AppIdHandler.LoadAgolAppId();
		}

		public Uri GetLogInDialogUri()
		{
			var uriBuilder = new UriBuilder(ArcGISOnlineOauthUri)
			{
				Query = string.Format(
					"client_id={0}&response_type=code&redirect_uri={1}",
					GetAppId(), ArcGISOnlineRedirectUriQuery)
			};
			return uriBuilder.Uri;
		}

		public Uri GetTokenUri(string authorizationCode)
		{
			var uriBuilder = new UriBuilder(ArcGISOnlineTokenUri)
			{
				Query = string.Format(
					"client_id={0}&grant_type=authorization_code&redirect_uri={1}&code={2}",
					GetAppId(), ArcGISOnlineRedirectUriQuery, authorizationCode)
			};
			return uriBuilder.Uri;
		}

		/// <summary>
		/// Einen OAuth-Token für eine Autorisation holen
		/// </summary>
		/// <param name="authorizationCode">Autorisations Code als Ergebnis eines erfolgreichen Logins</param>
		/// <exception cref="AuthenticationException">mit Message "Empty token generated" und "No token generated"</exception>
		public async Task<OAuthTokenCredential> GetToken(string authorizationCode)
		{
			var tokenWebResponse = await RequestUri(GetTokenUri(authorizationCode));
			return ParseResult(tokenWebResponse);
		}

		private static async Task<string> RequestUri(Uri uri)
		{
			// You need to add a reference to System.Net.Http to declare client.
			var client = new HttpClient();

			// GetStringAsync returns a Task<string>. That means that when you await the 
			// task you'll get a string (urlContents).
			return await client.GetStringAsync(uri.AbsoluteUri);
		}

		private OAuthTokenCredential ParseResult(string jsonWebResponse)
		{
			OAuth2TokenResult results;
			try
			{
				using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonWebResponse)))
				{
					var serializer = new DataContractJsonSerializer(typeof (OAuth2TokenResult));
					results = serializer.ReadObject(stream) as OAuth2TokenResult;
				}
			}
			catch (Exception)
			{
				results = null;
			}

			var oAuthTokenCredential = new OAuthTokenCredential();

			if (results != null && results.AccessToken != null)
			{
				// Token returned --> no error
				oAuthTokenCredential.Token = results.AccessToken;
				oAuthTokenCredential.OAuthRefreshToken = results.RefreshToken;
				oAuthTokenCredential.UserName = results.Username;

				if (results.ExpiresIn != null)
				{
					long expiresIn;
					Int64.TryParse(results.ExpiresIn, out expiresIn);
					oAuthTokenCredential.ExpirationDate = DateTime.UtcNow + TimeSpan.FromSeconds(expiresIn);
				}

				if (string.IsNullOrEmpty(oAuthTokenCredential.Token))
				{
					throw new AuthenticationException("Empty token generated");
				}
			}
			else
			{
				// Error
				string message = "No token generated";
				if (results != null && results.Error != null)
				{
					OAuth2TokenError tokenError = results.Error;
					if (tokenError != null)
					{
						if (tokenError.Message != null)
						{
							message = tokenError.Message;
						}
						if (tokenError.Details != null)
						{
							object messages = tokenError.Details;
							if (messages is string)
							{
								message += string.Format("{0}{1}", message.Length > 0 ? Environment.NewLine : string.Empty, messages);
							}
							else if (messages is IEnumerable)
							{
								message += string.Format("{0}{1}", message.Length > 0 ? Environment.NewLine : string.Empty,
									(messages as IEnumerable).OfType<String>().FirstOrDefault());
							}
						}
						if (tokenError.ErrorDescription != null)
						{
							message += string.Format("{0}{1}", message.Length > 0 ? Environment.NewLine : string.Empty,
								tokenError.ErrorDescription);
						}
					}
				}
				throw new AuthenticationException(message);
			}
			return oAuthTokenCredential;
		}

		#region nested classes only used internally

		[DataContract(Name = "OAuth2TokenError")]
		private class OAuth2TokenError
		{
			[DataMember(Name = "details")] public string Details;

			[DataMember(Name = "error_description")] public string ErrorDescription;
			[DataMember(Name = "message")] public string Message;
		}

		[DataContract(Name = "OAuth2TokenResult")]
		private class OAuth2TokenResult
		{
			[DataMember(Name = "access_token")] public string AccessToken;
			[DataMember(Name = "error")] public OAuth2TokenError Error;

			[DataMember(Name = "expires_in")] public string ExpiresIn;

			[DataMember(Name = "refresh_token")] public string RefreshToken;

			[DataMember(Name = "username")] public string Username;
		}

		#endregion
	}
}