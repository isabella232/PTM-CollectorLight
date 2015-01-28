using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Esri.ArcGISRuntime.Symbology;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.Annotations;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class LegendInfo : INotifyPropertyChanged
	{
		private readonly RelayCommand<LegendInfo> _createNewFeatureCommand;
		private readonly Symbol _esriSymbol;
		private readonly ImageSource _imageSymbol;
		private readonly string _label;
		private bool _isChecked;

		public LegendInfo(string label, ImageSource imageSymbol, Symbol esriSymbol,
			RelayCommand<LegendInfo> createNewFeatureCommand)
		{
			_label = label;
			_imageSymbol = imageSymbol;
			_esriSymbol = esriSymbol;
			_createNewFeatureCommand = createNewFeatureCommand;
		}

		public string Label
		{
			get { return _label; }
		}

		public ImageSource ImageSymbol
		{
			get { return _imageSymbol; }
		}

		public Symbol EsriSymbol
		{
			get { return _esriSymbol; }
		}

		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				_isChecked = value;
				RaisePropertyChangedEvent();
			}
		}

		public RelayCommand<LegendInfo> CreateNewFeatureCommand
		{
			get { return _createNewFeatureCommand; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChangedEvent()
		{
			// Get the call stack 
			StackTrace stackTrace = new StackTrace();

			// Get the calling method name 
			string callingMethodName = stackTrace.GetFrame(1).GetMethod().Name;

			// Check if the callingMethodName contains an underscore like in "set_SomeProperty" 
			if (callingMethodName.Contains("_"))
			{
				// Extract the property name 
				string propertyName = callingMethodName.Split('_')[1];

				if (PropertyChanged != null && propertyName != String.Empty)
				{
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			}
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}