using System.Collections.Generic;

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	public class WebMapItemJsonStructure
	{
		public class ApplicationProperties
		{
			public Viewing viewing { get; set; }
		}

		public class BaseMap
		{
			public List<BaseMapLayer> baseMapLayers { get; set; }
			public string title { get; set; }
		}

		public class BaseMapLayer
		{
			public string id { get; set; }
			public string layerType { get; set; }
			public int opacity { get; set; }
			public bool visibility { get; set; }
			public string url { get; set; }
			public bool showLegend { get; set; }
			public int minScale { get; set; }
			public int maxScale { get; set; }
		}

		public class BasemapGallery
		{
			public bool enabled { get; set; }
		}

		public class DrawingInfo
		{
			public Renderer renderer { get; set; }
		}

		public class FieldInfo
		{
			public string fieldName { get; set; }
			public string label { get; set; }
			public bool isEditable { get; set; }
			public string tooltip { get; set; }
			public bool visible { get; set; }
			public Format format { get; set; }
			public string stringFieldOption { get; set; }
		}

		public class Format
		{
			public int? places { get; set; }
			public bool? digitSeparator { get; set; }
			public string dateFormat { get; set; }
		}

		public class LayerDefinition
		{
			public DrawingInfo drawingInfo { get; set; }
		}

		public class Measure
		{
			public bool enabled { get; set; }
		}

		public class OperationalLayer
		{
			public string id { get; set; }
			public string layerType { get; set; }
			public string url { get; set; }
			public bool visibility { get; set; }
			public double opacity { get; set; }
			public string title { get; set; }
			public string itemId { get; set; }
			public LayerDefinition layerDefinition { get; set; }
			public PopupInfo popupInfo { get; set; }
			public bool showLegend { get; set; }
			public int minScale { get; set; }
			public int maxScale { get; set; }
		}

		public class PopupInfo
		{
			public string title { get; set; }
			public List<FieldInfo> fieldInfos { get; set; }
			public object description { get; set; }
			public bool showAttachments { get; set; }
			public List<object> mediaInfos { get; set; }
		}

		public class Renderer
		{
			public string type { get; set; }
			public Symbol symbol { get; set; }
		}

		public class RootObject
		{
			public List<OperationalLayer> operationalLayers { get; set; }
			public BaseMap baseMap { get; set; }
			public SpatialReference spatialReference { get; set; }
			public string version { get; set; }
			public ApplicationProperties applicationProperties { get; set; }
		}

		public class Routing
		{
			public bool enabled { get; set; }
		}

		public class Search
		{
			public bool enabled { get; set; }
			public bool disablePlaceFinder { get; set; }
			public string hintText { get; set; }
			public List<object> layers { get; set; }
		}

		public class SpatialReference
		{
			public int wkid { get; set; }
			public int latestWkid { get; set; }
		}

		public class Symbol
		{
			public int angle { get; set; }
			public int xoffset { get; set; }
			public double yoffset { get; set; }
			public string type { get; set; }
			public string url { get; set; }
			public string imageData { get; set; }
			public string contentType { get; set; }
			public int width { get; set; }
			public double height { get; set; }
		}

		public class Viewing
		{
			public Routing routing { get; set; }
			public BasemapGallery basemapGallery { get; set; }
			public Measure measure { get; set; }
			public Search search { get; set; }
		}
	}
}