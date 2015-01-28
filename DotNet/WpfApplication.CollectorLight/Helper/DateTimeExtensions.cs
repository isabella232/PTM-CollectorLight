using System;

namespace WpfApplication.CollectorLight.Helper
{
	public static class DateTimeExtensions
	{
		public static DateTime ConvertMillisecondsSince1970(long milliseconds)
		{
			var utcBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var dt = utcBaseTime.Add(new TimeSpan(milliseconds*TimeSpan.TicksPerMillisecond)).ToLocalTime();
			return dt;
		}
	}
}