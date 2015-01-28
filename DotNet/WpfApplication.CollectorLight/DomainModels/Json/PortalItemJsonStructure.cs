using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace WpfApplication.CollectorLight.DomainModels.Json
{
	/// <summary>
	/// This ist the Structure exactly as the AGOL services answers in json
	/// </summary>
	/// <see cref="http://json2csharp.com/"/>
	public class PortalItemJsonStructure
	{
		public string id { get; set; }
		public string owner { get; set; }
		public long created { get; set; }
		public long modified { get; set; }
		public string guid { get; set; }
		public string name { get; set; }
		public string title { get; set; }
		public string type { get; set; }
		public List<string> typeKeywords { get; set; }
		public string description { get; set; }
		public List<string> tags { get; set; }
		public string snippet { get; set; }
		public string thumbnail { get; set; }
		public object documentation { get; set; }
		public List<List<double>> extent { get; set; }
		public object spatialReference { get; set; }
		public string accessInformation { get; set; }
		public string licenseInfo { get; set; }
		public string culture { get; set; }
		public object properties { get; set; }
		public string url { get; set; }
		public string access { get; set; }
		public long size { get; set; }
		public List<object> appCategories { get; set; }
		public List<object> industries { get; set; }
		public List<object> languages { get; set; }
		public object largeThumbnail { get; set; }
		public object banner { get; set; }
		public List<object> screenshots { get; set; }
		public bool listed { get; set; }
		public bool commentsEnabled { get; set; }
		public int numComments { get; set; }
		public int numRatings { get; set; }
		public double avgRating { get; set; }
		public int numViews { get; set; }
	}
}

// ReSharper restore InconsistentNaming