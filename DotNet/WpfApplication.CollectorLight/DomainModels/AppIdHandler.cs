using System;
using System.Windows;
using Microsoft.VisualBasic;
using WpfApplication.CollectorLight.Properties;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class AppIdHandler
	{
		public static event Action AgolAppIdChanged = delegate { };

		public static string ChangeAgolAppId()
		{
			try
			{
				string input =
					Interaction.InputBox(
						"Bitte geben Sie eine Client Id an, die die Anwendung für die Authentizierung gegen ArcGIS Online verwenden soll.",
						"AppID",
						Settings.Default["ArcGISOnlineAppID"] as string);

				if (string.IsNullOrEmpty(input) || input.Length != 16)
				{
					MessageBox.Show("Die ID scheint keine gültige Client Id zu sein, wird aber trotzdem gespeichert und verwendet.");
				}

				if (input == (string) Settings.Default["ArcGISOnlineAppID"])
				{
					return (string) Settings.Default["ArcGISOnlineAppID"];
				}

				Settings.Default["ArcGISOnlineAppID"] = input;
				Settings.Default.Save(); // Saves settings in application configuration

				AgolAppIdChanged();
				return input;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
				return null;
			}
		}

		public static string LoadAgolAppId()
		{
			try
			{
				var agolAppId = (string) Settings.Default["ArcGISOnlineAppID"];
				if (!string.IsNullOrEmpty(agolAppId))
				{
					return agolAppId;
				}
			}
			catch (Exception)
			{
			}

			return ChangeAgolAppId();
		}
	}
}