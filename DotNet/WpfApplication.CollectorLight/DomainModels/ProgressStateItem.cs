namespace WpfApplication.CollectorLight.DomainModels
{
	public class ProgressStateItem
	{
		public ProgressStateItem()
		{
			Message = string.Empty;
			IsActive = false;
			Percentage = 0;
		}

		public ProgressStateItem(string message, bool isActive, double percentage)
		{
			Message = message;
			IsActive = isActive;
			Percentage = percentage;
		}

		public string Message { get; set; }
		public bool IsActive { get; set; }
		public double Percentage { get; set; }
	}
}