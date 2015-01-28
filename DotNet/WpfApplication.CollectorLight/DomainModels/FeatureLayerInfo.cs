using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Esri.ArcGISRuntime.Layers;
using WpfApplication.CollectorLight.Annotations;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class FeatureLayerInfo : INotifyPropertyChanged
	{
		private readonly FeatureLayer _featureLayer;
		private readonly bool _isEditable;
		private readonly bool _isSyncable;
		private readonly List<LegendInfo> _legendInfos;
		private long _edits;


		public FeatureLayerInfo(FeatureLayer featureLayer, bool isEditable, bool isSyncable, long edits,
			List<LegendInfo> legendInfos)
		{
			_featureLayer = featureLayer;
			_isEditable = isEditable;
			_isSyncable = isSyncable;
			_edits = edits;
			_legendInfos = legendInfos;
		}

		public FeatureLayer FeatureLayer
		{
			get { return _featureLayer; }
		}

		public bool IsEditable
		{
			get { return _isEditable; }
		}

		public bool IsSyncable
		{
			get { return _isSyncable; }
		}

		public long Edits
		{
			get { return _edits; }
			set
			{
				_edits = value;
				RaisePropertyChanged();
				RaisePropertyChanged("HasEdits");
			}
		}

		public bool HasEdits
		{
			get { return _edits > 0; }
		}

		public List<LegendInfo> LegendInfos
		{
			get { return _legendInfos; }
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}