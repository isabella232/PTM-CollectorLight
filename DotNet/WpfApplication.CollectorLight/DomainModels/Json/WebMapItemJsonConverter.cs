using System;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	public class WebMapItemJsonConverter
	{
		public static WebMapItemJsonStructure.RootObject CreateWebMapJsonStructure(string jsonString)
		{
			return JsonConvert.DeserializeObject<WebMapItemJsonStructure.RootObject>(jsonString);
		}

		public static string GetWebMapJsonString(Uri uriWithJsonResponse)
		{
			WebRequest request = WebRequest.Create(uriWithJsonResponse);
			WebResponse response;
			try
			{
				response = request.GetResponse();
			}
			catch (Exception ex)
			{
				//Fehlerausgabe wünschenswert
				MessageBox.Show(ex.ToString(), string.Format("Fehler beim Aufruf von {0}", uriWithJsonResponse.AbsolutePath));
				return null;
			}

			var stream = response.GetResponseStream();
			if (stream == null) return null;
			var jsonString = new StreamReader(stream).ReadToEnd();
			return jsonString;
		}
	}
}