using System;
using System.Linq;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using WpfApplication.CollectorLight.Helper;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class ArcGisPortalWebMapItem
	{
		private readonly ArcGISPortalItem _arcGisPortalItem;
		private readonly bool _isEditable;
		private readonly bool _isSyncable;
		private readonly WebMap _webMap;

		public ArcGisPortalWebMapItem(ArcGISPortalItem arcGisPortalItem, WebMap webMap, bool isEditable, bool isSyncable)
		{
			_arcGisPortalItem = arcGisPortalItem;
			_webMap = webMap;
			_isEditable = isEditable;
			_isSyncable = isSyncable;
		}

		public ArcGISPortalItem ArcGisPortalItem
		{
			get { return _arcGisPortalItem; }
		}

		public WebMap WebMap
		{
			get { return _webMap; }
		}

		public bool IsEditable
		{
			get { return _isEditable; }
		}

		public bool IsSyncable
		{
			get { return _isSyncable; }
		}

		public Uri WebMapItemJsonUri
		{
			get
			{
				try
				{
					var credential = IdentityManager.Current.Credentials
						.OfType<ArcGISTokenCredential>()
						.FirstOrDefault(c => PortalItemUri.ToString().StartsWith(c.ServiceUri));
					var portalItemUri = PortalItemUri.Append("/data").Append(string.Format("?f=pjson&token={0}", credential.Token));
					return portalItemUri;
				}
				catch
				{
					return null;
				}
			}
		}

		public Uri PortalItemJsonUri
		{
			get
			{
				try
				{
					var credential = IdentityManager.Current.Credentials
						.OfType<ArcGISTokenCredential>()
						.FirstOrDefault(c => PortalItemUri.ToString().StartsWith(c.ServiceUri));
					var portalItemUri = PortalItemUri.Append(string.Format("?f=pjson&token={0}", credential.Token));
					return portalItemUri;
				}
				catch
				{
					return null;
				}
			}
		}

		public Uri PortalItemUri
		{
			get
			{
				var host = _arcGisPortalItem.ArcGISPortal.Uri;
				if (!host.AbsolutePath.Contains(@"sharing"))
				{
					host.Append(@"/sharing/rest/");
				}
				return host.Append(@"/content/items/").Append(_arcGisPortalItem.Id);
			}
		}
	}
}