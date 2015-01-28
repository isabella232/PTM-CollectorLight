using System;
using System.IO;
using System.Linq;
using Esri.ArcGISRuntime.Data;
using WpfApplication.CollectorLight.DomainModels.Json;

namespace WpfApplication.CollectorLight.Helper
{
	public static class GeodatabaseExtensions
	{
		public static Uri GetServiceUri(this Geodatabase geodatabase)
		{
			// path to geodatabase
			var geodatabaseDirectory = Path.GetDirectoryName(geodatabase.Path);
			var jsonDirectory = Directory.GetParent(geodatabaseDirectory).ToString();

			var path = jsonDirectory + "\\" + "webmapitem.json";

			string jsonString;
			using (var sr = new StreamReader(path))
			{
				jsonString = sr.ReadToEnd();
			}

			var json = new WebMapItemJson(jsonString);
			var url = json.OperationalLayers.First().url;
			url = url.Substring(0, url.LastIndexOf('/'));
			return new Uri(url);
		}
	}
}