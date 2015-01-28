using System.IO;
using WpfApplication.CollectorLight.DomainModels.Json;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class OfflineMapItem
	{
		public static string GeodatabaseFilenameExtension = ".geodatabase";
		public static string BasemapFilenameExtension = ".tpk";
		public static string ThumbnailName = "icon.png";
		public static string Type = "Offline Map";

		private readonly string _offlineMapName;
		private readonly string _offlineMapPath;

		public OfflineMapItem(PortalItemJson portalItemJson, string offlineMapName, string offlineMapPath)
		{
			PortalItemJson = portalItemJson;
			_offlineMapName = offlineMapName;
			_offlineMapPath = offlineMapPath;
		}

		public string OfflineMapName
		{
			get { return _offlineMapName; }
		}

		public string OfflineMapPath
		{
			get { return _offlineMapPath; }
		}

		public string WebMapPath
		{
			get { return Directory.GetParent(_offlineMapPath).FullName; }
		}

		public string ThumbnailPath
		{
			get
			{
				//Im Offline Cache Ordner ein individueller Ausschnitt vorhanden?
				var thumbnailPath = Path.Combine(OfflineMapPath, ThumbnailName);
				if (File.Exists(thumbnailPath))
				{
					return thumbnailPath;
				}

				//Im WebMap Ordner ein allgemeines Icon vorhanden?
				thumbnailPath = Path.Combine(WebMapPath, ThumbnailName);
				if (File.Exists(thumbnailPath))
				{
					return thumbnailPath;
				}
				return string.Empty;
			}
		}

		public PortalItemJson PortalItemJson { get; private set; }

		public string WebMapId
		{
			get { return PortalItemJson.Id; }
		}
	}
}