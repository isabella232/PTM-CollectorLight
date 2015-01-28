using System;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Geometry;
using WpfApplication.CollectorLight.Helper;

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	/// <summary>
	/// This class represents the PortalItem structure similar to the json structure from AGOL
	/// But it converts some properties to a more fitting datatype
	/// </summary>
	public class PortalItemJson
	{
		private readonly PortalItemJsonStructure _portalItemJsonStructure;

		public PortalItemJson(string jsonString)
		{
			_portalItemJsonStructure = PortalItemJsonConverter.CreatePortalItemJsonStructure(jsonString);
		}

		public string Id
		{
			get { return _portalItemJsonStructure.id ?? string.Empty; }
		}

		public string Owner
		{
			get { return _portalItemJsonStructure.owner ?? string.Empty; }
		}

		public DateTime Created
		{
			get { return DateTimeExtensions.ConvertMillisecondsSince1970(_portalItemJsonStructure.created); }
		}

		public DateTime Modified
		{
			get { return DateTimeExtensions.ConvertMillisecondsSince1970(_portalItemJsonStructure.modified); }
		}

		public string Guid
		{
			get { return _portalItemJsonStructure.guid ?? string.Empty; }
		}

		public string Name
		{
			get { return _portalItemJsonStructure.name ?? string.Empty; }
		}

		public string Title
		{
			get { return _portalItemJsonStructure.title ?? string.Empty; }
		}

		public string Type
		{
			get { return _portalItemJsonStructure.type ?? string.Empty; }
		}

		public List<string> TypeKeywords
		{
			get { return _portalItemJsonStructure.typeKeywords; }
		}

		public string Description
		{
			get { return _portalItemJsonStructure.description ?? Snippet; }
		}

		public List<string> Tags
		{
			get { return _portalItemJsonStructure.tags; }
		}

		public string Snippet
		{
			get { return _portalItemJsonStructure.snippet ?? string.Empty; }
		}

		public string Thumbnail
		{
			get { return _portalItemJsonStructure.thumbnail ?? string.Empty; }
		}

		public Envelope Extent
		{
			get
			{
				//sample:
				// "extent": [
				//	[
				//	  11.6739,
				//	  48.1271
				//	],
				//	[
				//	  11.7372,
				//	  48.1551
				//	]
				//  ]
				return new Envelope(_portalItemJsonStructure.extent[0][0], _portalItemJsonStructure.extent[0][1],
					_portalItemJsonStructure.extent[1][0], _portalItemJsonStructure.extent[1][1]);
			}
		}

		public string Url
		{
			get { return _portalItemJsonStructure.url ?? string.Empty; }
		}

		public string Access
		{
			get { return _portalItemJsonStructure.access ?? string.Empty; }
		}

		public long Size
		{
			get { return _portalItemJsonStructure.size; }
		}

		public object LargeThumbnail
		{
			get { return _portalItemJsonStructure.largeThumbnail ?? string.Empty; }
		}

		public override string ToString()
		{
			return string.Format("WebMapId {0}, Titel {1}", Id, Title);
		}
	}
}