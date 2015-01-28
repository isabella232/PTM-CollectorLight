using System.ComponentModel;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.Annotations;

namespace WpfApplication.CollectorLight.DomainModels
{
	public class OfflineMapListBoxItem : INotifyPropertyChanged
	{
		private readonly RelayCommand<OfflineMapItem> _deleteOfflineMapCommand;
		private readonly RelayCommand<OfflineMapItem> _loadOfflineMapCommand;
		private readonly OfflineMapItem _offlineMapItem;
		private readonly RelayCommand<OfflineMapItem> _syncOfflineMapCommand;
		private ProgressStateItem _progressStateItem;


		public OfflineMapListBoxItem(OfflineMapItem offlineMapItem, RelayCommand<OfflineMapItem> loadOfflineMapCommand,
			RelayCommand<OfflineMapItem> syncOfflineMapCommand, RelayCommand<OfflineMapItem> deleteOfflineMapCommand)
		{
			_offlineMapItem = offlineMapItem;
			ProgressStateItem = new ProgressStateItem();
			_loadOfflineMapCommand = loadOfflineMapCommand;
			_syncOfflineMapCommand = syncOfflineMapCommand;
			_deleteOfflineMapCommand = deleteOfflineMapCommand;
		}

		public OfflineMapItem OfflineMapItem
		{
			get { return _offlineMapItem; }
		}

		public ProgressStateItem ProgressStateItem
		{
			get { return _progressStateItem; }
			set
			{
				_progressStateItem = value;
				RaisePropertyChanged();
			}
		}

		public RelayCommand<OfflineMapItem> LoadOfflineMapCommand
		{
			get { return _loadOfflineMapCommand; }
		}

		public RelayCommand<OfflineMapItem> SyncOfflineMapCommand
		{
			get { return _syncOfflineMapCommand; }
		}

		public RelayCommand<OfflineMapItem> DeleteOfflineMapCommand
		{
			get { return _deleteOfflineMapCommand; }
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