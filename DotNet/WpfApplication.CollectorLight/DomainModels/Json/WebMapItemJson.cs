using System.Collections.Generic;

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	public class WebMapItemJson
	{
		private readonly WebMapItemJsonStructure.RootObject _webMapItemJsonStructure;

		public WebMapItemJson(string jsonString)
		{
			_webMapItemJsonStructure = WebMapItemJsonConverter.CreateWebMapJsonStructure(jsonString);
		}

		public List<WebMapItemJsonStructure.OperationalLayer> OperationalLayers
		{
			get { return _webMapItemJsonStructure.operationalLayers; }
		}

		public WebMapItemJsonStructure.BaseMap BaseMap
		{
			get { return _webMapItemJsonStructure.baseMap; }
		}

		public WebMapItemJsonStructure.SpatialReference SpatialReference
		{
			get { return _webMapItemJsonStructure.spatialReference; }
		}

		public string Version
		{
			get { return _webMapItemJsonStructure.version; }
		}

		public WebMapItemJsonStructure.ApplicationProperties ApplicationProperties
		{
			get { return _webMapItemJsonStructure.applicationProperties; }
		}
	}
}