using System;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	/// <summary>
	/// This class converts a PortalItem structure from string to json and back
	/// </summary>
	public class PortalItemJsonConverter
	{
		public static PortalItemJsonStructure CreatePortalItemJsonStructure(string jsonString)
		{
			return JsonConvert.DeserializeObject<PortalItemJsonStructure>(jsonString);
		}

		//public static PortalItemJsonStructure CreatePortalItemJsonStructure(Uri uriWithJsonResponse)
		//{
		//	var jsonString = GetPortalItemJsonString(uriWithJsonResponse);
		//	return CreatePortalItemJsonStructure(jsonString);
		//}

		public static string GetPortalItemJsonString(Uri uriWithJsonResponse)
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