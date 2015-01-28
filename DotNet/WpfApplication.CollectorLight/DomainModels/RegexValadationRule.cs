using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class RegexValidationRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var regex = new Regex(@"[a-zA-Z0-9]");
			// Logic to determine if the TextBox contains a valid string goes here
			// Maybe a reg ex that only matches alphanumerics and spaces
			if (regex.IsMatch((string) value))
				return ValidationResult.ValidResult;
			return new ValidationResult(false, "!");
		}
	}
}