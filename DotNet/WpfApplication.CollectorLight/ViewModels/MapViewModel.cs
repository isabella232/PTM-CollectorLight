using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.WebMap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;
using WpfApplication.CollectorLight.Helper;

namespace WpfApplication.CollectorLight.ViewModels
{
	public class MapViewModel : ViewModelBase
	{
		private readonly IModel _model;

		// private variable to track cancellation requests
		private MapView _currentEsriMapView;
		private OfflineMapItem _currentOfflineMapItem;
		private ObservableCollection<FeatureLayerInfo> _featureLayerInfoItems;
		private bool _isInProgress;
		private CancellationTokenSource _syncCancellationTokenSource;

		public MapViewModel(IModel model)
		{
			_model = model;
			FeatureLayerInfoItems = new ObservableCollection<FeatureLayerInfo>();
			InitializeModelEvents();
			InitializeRelayCommands();
		}

		public MapView CurrentEsriMapView
		{
			get { return _currentEsriMapView; }
			set
			{
				_currentEsriMapView = value;
				RaisePropertyChanged("CurrentEsriMapView");
			}
		}

		public RelayCommand<LegendInfo> CreateNewFeatureCommand { get; private set; }

		public ObservableCollection<FeatureLayerInfo> FeatureLayerInfoItems
		{
			get { return _featureLayerInfoItems; }
			set
			{
				_featureLayerInfoItems = value;
				RaisePropertyChanged("FeatureLayerInfoItems");
			}
		}

		public bool IsInProgress
		{
			get { return _isInProgress; }
			set
			{
				_isInProgress = value;
				RaisePropertyChanged("IsInProgress");
			}
		}

		public bool IsSyncPossible
		{
			get { return CheckSyncState(false); }
		}

		private void InitializeModelEvents()
		{
			_model.LoadWebMap += async (p, q) =>
			{
				await Reset();
				IsInProgress = true;
				await LoadWebMap(p, q);
				await CreateToc();
				IsInProgress = false;
				_model.SetSyncState(CheckSyncState(false));
			};

			_model.LoadOfflineMap += async p =>
			{
				await Reset();
				IsInProgress = true;
				await LoadOfflineMap(p);
				await CreateToc();
				IsInProgress = false;
				_model.SetSyncState(CheckSyncState(false));
			};

			_model.SyncOfflineMap += async p =>
			{
				//TODO Reset sinnlos, Button disabled, wenn _model.IsSyncPossible == false, wenn map geladen, legende aktualisieren

				await Reset();
				IsInProgress = true;

				_model.SetSyncState(CheckSyncState(true));

				_model.TriggerProgressChangedEvent(p.OfflineMapPath,
					new ProgressStateItem(string.Format("Sync Geodatabase ..."), true, 0));
				_model.SetMessageInfo("--- Start sync ---");

				await SyncOfflineMap(p);
				IsInProgress = false;
				_model.SetSyncState(CheckSyncState(false));
				_model.TriggerProgressChangedEvent(p.OfflineMapPath, new ProgressStateItem(string.Format(""), false, 0));
			};
			_model.DeleteOfflineMap += async p =>

			{
				await Reset();
				IsInProgress = true;
				await DeleteOfflineMap(p);
				IsInProgress = false;
				_model.SetSyncState(CheckSyncState(false));
			};

			_model.ResetMapView += async () => { await Reset(); };
		}

		private void InitializeRelayCommands()
		{
			CreateNewFeatureCommand = new RelayCommand<LegendInfo>(async p =>
			{
				SetLegendInfosCheckState(p);
				var featureLayerInfo = GetFeatureLayerInfoByLegendInfo(p);
				try
				{
					await CreateNewFeature(featureLayerInfo, p);
					_model.SetSyncState(CheckSyncState(false));
				}
				catch
				{
				}

				UpdateToc(featureLayerInfo);
			});
		}

		private async void CurrentEsriMapViewTapped(object sender, MapViewInputEventArgs e)
		{
			// Ignore tap events while in edit mode so we do not interfere with edit geometry.
			if (CurrentEsriMapView.Editor.IsActive)
				return;

			foreach (var layer in CurrentEsriMapView.Map.Layers)
			{
				if (!(layer is FeatureLayer))
					continue;

				var featureLayer = layer as FeatureLayer;
				featureLayer.ClearSelection();

				try
				{
					// Performs hit test on layer to select feature.
					var features = await featureLayer.HitTestAsync(CurrentEsriMapView, e.Position);
					if (features == null || !features.Any())
						continue;
					var featureId = features.FirstOrDefault();
					featureLayer.SelectFeatures(new[] {featureId});
					var feature = await featureLayer.FeatureTable.QueryAsync(featureId);
				}
				catch
				{
				}
			}
		}

		private FeatureLayerInfo GetFeatureLayerInfoByLegendInfo(LegendInfo legendInfo)
		{
			return
				FeatureLayerInfoItems.FirstOrDefault(
					featureLayerInfoItem => Enumerable.Contains(featureLayerInfoItem.LegendInfos, legendInfo));
		}

		private void SetLegendInfosCheckState(LegendInfo legendInfo)
		{
			foreach (var featureLayerInfoItem in FeatureLayerInfoItems)
			{
				foreach (var info in featureLayerInfoItem.LegendInfos.Where(info => !info.Equals(legendInfo)))
				{
					info.IsChecked = false;
				}
			}
		}

		private async Task LoadOfflineMap(OfflineMapItem offlineMapItem)
		{
			_currentOfflineMapItem = offlineMapItem;
			var basemapName = GetBasemapNameFromDirectory(offlineMapItem.OfflineMapPath);
			var arcGisLocalTiledLayer =
				new ArcGISLocalTiledLayer(string.Format("{0}\\{1}{2}", offlineMapItem.OfflineMapPath, basemapName,
					OfflineMapItem.BasemapFilenameExtension));
			arcGisLocalTiledLayer.DisplayName = basemapName;
			var offlineMap = new Map();
			offlineMap.Layers.Add(arcGisLocalTiledLayer);

			var files = Directory.GetFiles(offlineMapItem.OfflineMapPath,
				string.Format("*{0}", OfflineMapItem.GeodatabaseFilenameExtension));
			foreach (var file in files)
			{
				var gdbFile = file.Replace("/", @"\");
				var featureLayerList = await CreateOfflineFeatureLayersAsync(gdbFile);

				foreach (var featureLayer in featureLayerList)
				{
					var geodatabaseFeatureTable = featureLayer.FeatureTable as GeodatabaseFeatureTable;
					featureLayer.DisplayName = geodatabaseFeatureTable.ServiceInfo.Name;
					featureLayer.IsVisible = geodatabaseFeatureTable.ServiceInfo.DefaultVisibility;
					offlineMap.Layers.Add(featureLayer);
				}

				break;
			}

			var mapView = new MapView {Map = offlineMap};
			CurrentEsriMapView = mapView;
			CurrentEsriMapView.MapViewTapped += CurrentEsriMapViewTapped;
			await Task.Delay(TimeSpan.FromSeconds(2));
		}

		private async Task SyncOfflineMap(OfflineMapItem offlineMapItem)
		{
			var gdbFiles = Directory.GetFiles(offlineMapItem.OfflineMapPath,
				string.Format("*{0}", OfflineMapItem.GeodatabaseFilenameExtension));
			foreach (var gdbFile in gdbFiles)
			{
				var gdbPath = gdbFile.Replace("/", @"\");
				var geodatabase = await Geodatabase.OpenAsync(gdbPath);
				if (!geodatabase.FeatureTables.Any(table => table.HasEdits))
				{
					// nothing to sync..
					continue;
				}

				_model.SetMessageInfo("Sync geodatabase...");
				var serviceUri = geodatabase.GetServiceUri();
				var geodatabaseSyncTask = new GeodatabaseSyncTask(serviceUri);
				var statusCallback =
					new Progress<GeodatabaseStatusInfo>(
						statusInfo => _model.SetMessageInfo(string.Format("Current Status: {0}", statusInfo.Status)));

				var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();

				try
				{
					// At this point, we are submitting a job. Therefore, the await returns pretty fast.
					await geodatabaseSyncTask.SyncGeodatabaseAsync(
						new SyncGeodatabaseParameters
						{
							RollbackOnFailure = true,
							SyncDirection = SyncDirection.Upload,
							UnregisterGeodatabase = false
						},
						geodatabase,
						(geodatabaseStatusInfo, exc) =>
						{
							if (geodatabaseStatusInfo != null)
							{
								tcs.SetResult(geodatabaseStatusInfo);
							}
							if (exc != null)
							{
								tcs.SetException(exc);
							}
						},
						uploadResult => { _model.SetMessageInfo(string.Format("UploadResult: {0}", uploadResult.Success)); },
						TimeSpan.FromSeconds(5),
						statusCallback,
						CancellationToken.None);

					// This is the call we are really waiting for..
					await tcs.Task;
					_model.SetMessageInfo(string.Format("Finished Status: {0}", tcs.Task.Result.Status));
				}
				catch (Exception exc)
				{
					_model.SetMessageInfo(string.Format("Executing SyncGeodatabaseAsync failed: {0}", exc.Message));
				}
				_model.SetMessageInfo("...done syncing geodatabase");
			}
		}

		private async Task DeleteOfflineMap(OfflineMapItem offlineMapItem)
		{
			// TODO Ensure, map isn't loaded
			// TODO Ensure, user is logged on

			_model.SetMessageInfo(string.Format("Unregister Geodatabases..."));
			var gdbFiles = Directory.GetFiles(offlineMapItem.OfflineMapPath,
				string.Format("*{0}", OfflineMapItem.GeodatabaseFilenameExtension));
			foreach (var gdbFile in gdbFiles)
			{
				_model.SetMessageInfo(string.Format("\tUnregister Geodatabase '{0}'...", gdbFile));
				var gdbPath = gdbFile.Replace("/", @"\");
				var geodatabase = await Geodatabase.OpenAsync(gdbPath);

				var serviceUri = geodatabase.GetServiceUri();
				var geodatabaseSyncTask = new GeodatabaseSyncTask(serviceUri);
				var result = await geodatabaseSyncTask.UnregisterGeodatabaseAsync(geodatabase);
				_model.SetMessageInfo(string.Format("\t...Geodatabase {0} {1}successfully unregistered", gdbFile,
					result.Success ? "" : "NOT "));

				// Workaround to release file handle, as Geodatabase does not implement IDisposable
				geodatabase = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				File.Delete(gdbFile);
			}

			// delete directory, including basemap
			Directory.Delete(offlineMapItem.OfflineMapPath, true);
		}

		private string GetBasemapNameFromDirectory(string directory)
		{
			var file = Directory.GetFiles(directory, string.Format("*{0}", OfflineMapItem.BasemapFilenameExtension))[0];
			var lastOrDefault = file.Split('\\').LastOrDefault();
			if (lastOrDefault != null)
			{
				return lastOrDefault.Replace(OfflineMapItem.BasemapFilenameExtension, string.Empty);
			}
			return "Basemap";
		}

		private async Task<List<FeatureLayer>> CreateOfflineFeatureLayersAsync(string gdbPath)
		{
			try
			{
				var geodatabase = await Geodatabase.OpenAsync(gdbPath);

				if (!geodatabase.FeatureTables.Any())
				{
					_model.SetMessageInfo("Downloaded geodatabase has no feature tables.");
					return null;
				}

				var featureLayerList = new List<FeatureLayer>();

				foreach (var gdbFeatureTable in geodatabase.FeatureTables)
				{
					var featureLayer = new FeatureLayer
					{
						ID = gdbFeatureTable.Name,
						DisplayName = string.Format("{0} ({1})", gdbFeatureTable.Name, gdbFeatureTable.RowCount),
						FeatureTable = gdbFeatureTable
					};

					featureLayerList.Add(featureLayer);
				}

				return featureLayerList;
			}
			catch (Exception ex)
			{
				_model.SetMessageInfo("Error creating feature layer: " + ex.Message);
				return null;
			}
		}

		private async Task LoadWebMap(WebMap webMap, ArcGISPortal arcGisPortal)
		{
			var vm = await WebMapViewModel.LoadAsync(webMap, arcGisPortal);
			var mapView = new MapView {Map = vm.Map};
			CurrentEsriMapView = mapView;
			CurrentEsriMapView.MapViewTapped += CurrentEsriMapViewTapped;
		}

		private async Task Reset()
		{
			CurrentEsriMapView = null;
			_model.SetSyncState(CheckSyncState(true));
			_currentOfflineMapItem = null;
			await CreateToc();
			IsInProgress = false;
		}

		#region Edit

		private async Task CreateNewFeature(FeatureLayerInfo featureLayerInfo, LegendInfo legendInfo)
		{
			if (CurrentEsriMapView.Editor.IsActive)
			{
				CurrentEsriMapView.Editor.Cancel.Execute(null);
			}
			Graphic graphic = null;

			switch (featureLayerInfo.FeatureLayer.FeatureTable.GeometryType)
			{
				case GeometryType.Unknown:
					break;
				case GeometryType.Point:
					graphic = await CreateGraphicAsync(legendInfo.EsriSymbol, DrawShape.Point);
					break;
				case GeometryType.Polyline:
					graphic = await CreateGraphicAsync(legendInfo.EsriSymbol, DrawShape.Polyline);
					break;
				case GeometryType.Polygon:
					graphic = await CreateGraphicAsync(legendInfo.EsriSymbol, DrawShape.Polygon);
					break;
				case GeometryType.Envelope:
					break;
			}


			if (featureLayerInfo.FeatureLayer.FeatureTable is GeodatabaseFeatureTable)
			{
				var table = featureLayerInfo.FeatureLayer.FeatureTable as GeodatabaseFeatureTable;
				//_model.SetMessageInfo("Table was not found in the local geodatabase.");
				var feature = new GeodatabaseFeature(table.Schema) {Geometry = graphic.Geometry,};

				if (feature.Schema.Fields.Any(fld => fld.Name == table.ServiceInfo.DisplayField))
					feature.Attributes[table.ServiceInfo.DisplayField] = legendInfo.Label;

				await table.AddAsync(feature);
			}

			if (featureLayerInfo.FeatureLayer.FeatureTable is ServiceFeatureTable)
			{
				var table = featureLayerInfo.FeatureLayer.FeatureTable as ServiceFeatureTable;

				var feature = new GeodatabaseFeature(table.Schema) {Geometry = graphic.Geometry,};

				if (feature.Schema.Fields.Any(fld => fld.Name == table.ServiceInfo.DisplayField))
					feature.Attributes[table.ServiceInfo.DisplayField] = legendInfo.Label;

				await table.AddAsync(feature);
			}
		}

		private async Task<Graphic> CreateGraphicAsync(Symbol symbol, DrawShape drawShape)
		{
			try
			{
				// wait for user to draw the shape
				var geometry = await CurrentEsriMapView.Editor.RequestShapeAsync(drawShape, symbol);

				// add the new graphic to the graphic layer
				var graphic = new Graphic(geometry, symbol);

				return graphic;
			}
			catch (TaskCanceledException)
			{
			}
			catch (Exception ex)
			{
				_model.SetMessageInfo("Error drawing graphic: " + ex.Message);
			}
			return null;
		}

		#endregion

		#region TOC

		private async Task CreateToc()
		{
			FeatureLayerInfoItems.Clear();
			if (CurrentEsriMapView == null) return;

			foreach (var layer in CurrentEsriMapView.Map.Layers)
			{
				if (!(layer is FeatureLayer)) continue;

				var featureLayer = layer as FeatureLayer;
				long edits = 0;
				FeatureServiceLayerInfo serviceInfo = null;
				if (featureLayer.FeatureTable is GeodatabaseFeatureTable)
				{
					var table = featureLayer.FeatureTable as GeodatabaseFeatureTable;
					if (table.HasEdits)
					{
						edits = table.AddedFeaturesCount + table.DeletedFeaturesCount + table.UpdatedFeaturesCount;
					}

					serviceInfo = table.ServiceInfo;
				}

				if (featureLayer.FeatureTable is ServiceFeatureTable) //online
				{
					var table = featureLayer.FeatureTable as ServiceFeatureTable;
					if (table.HasEdits)
					{
						edits = table.AddedFeaturesCount + table.DeletedFeaturesCount + table.UpdatedFeaturesCount;
					}

					serviceInfo = table.ServiceInfo;
				}


				bool isEditable = false;
				bool isSyncable = false;
				foreach (var capability in serviceInfo.Capabilities)
				{
					if (capability == "Editing")
					{
						isEditable = true;
					}
					if (capability == "Sync")
					{
						isSyncable = true;
					}
				}

				if (isEditable)
				{
					// var test = featureLayer.FeatureTable.ServiceInfo.DrawingInfo.Renderer as SimpleRenderer;
					// UniqueValueInfoCollection uniqueValueInfoCollection = test.;

					var legendInfosList = new List<LegendInfo>();

					if (serviceInfo.DrawingInfo.Renderer is SimpleRenderer)
					{
						var renderer =
							serviceInfo.DrawingInfo.Renderer as SimpleRenderer;
						var imageSource = await renderer.Symbol.CreateSwatchAsync();
						var legendInfo = new LegendInfo(renderer.Label, imageSource, renderer.Symbol, CreateNewFeatureCommand);
						legendInfosList.Add(legendInfo);
					}
					else if (serviceInfo.DrawingInfo.Renderer is UniqueValueRenderer)
					{
						var renderer = serviceInfo.DrawingInfo.Renderer as UniqueValueRenderer;
						foreach (var info in renderer.Infos)
						{
							var imageSource = await info.Symbol.CreateSwatchAsync();
							legendInfosList.Add(new LegendInfo(info.Label, imageSource, info.Symbol, CreateNewFeatureCommand));
						}
					}
					else if (serviceInfo.DrawingInfo.Renderer is ClassBreaksRenderer)
					{
						var renderer = serviceInfo.DrawingInfo.Renderer as ClassBreaksRenderer;

						foreach (var info in renderer.Infos)
						{
							var imageSource = await info.Symbol.CreateSwatchAsync();
							legendInfosList.Add(new LegendInfo(info.Label, imageSource, info.Symbol, CreateNewFeatureCommand));
						}
					}

					var featureLayerEditInfoItem = new FeatureLayerInfo(featureLayer, isEditable, isSyncable, edits, legendInfosList);
					FeatureLayerInfoItems.Add(featureLayerEditInfoItem);
				}
			}
		}

		private void UpdateToc(FeatureLayerInfo featureLayerInfo)
		{
			if (CurrentEsriMapView == null) return;

			long edits = 0;
			if (featureLayerInfo.FeatureLayer.FeatureTable is GeodatabaseFeatureTable)
			{
				var table = featureLayerInfo.FeatureLayer.FeatureTable as GeodatabaseFeatureTable;
				if (table.HasEdits)
				{
					edits = table.AddedFeaturesCount + table.DeletedFeaturesCount + table.UpdatedFeaturesCount;
				}
			}

			if (featureLayerInfo.FeatureLayer.FeatureTable is ServiceFeatureTable) //online
			{
				var table = featureLayerInfo.FeatureLayer.FeatureTable as ServiceFeatureTable;
				if (table.HasEdits)
				{
					edits = table.AddedFeaturesCount + table.DeletedFeaturesCount + table.UpdatedFeaturesCount;
				}
			}
			featureLayerInfo.Edits = edits;
		}

		#endregion

		#region Sync

		private bool CheckSyncState(bool forceNotSyncable)
		{
			if (CurrentEsriMapView == null) return false;
			if (forceNotSyncable) return false;

			foreach (var layer in CurrentEsriMapView.Map.Layers)
			{
				if (layer is FeatureLayer)
				{
					var featureLayer = layer as FeatureLayer;

					if (featureLayer.FeatureTable is GeodatabaseFeatureTable)
					{
						var table = featureLayer.FeatureTable as GeodatabaseFeatureTable;
						if (table.HasEdits) return true;
					}
					if (featureLayer.FeatureTable is ServiceFeatureTable)
					{
						var table = featureLayer.FeatureTable as ServiceFeatureTable;
						if (table.HasEdits) return true;
					}
				}
			}

			return false;
		}

		private async Task<Uri> GetFeatureServiceUrlForSync(FeatureLayerInfo featureLayerInfo)
		{
			var arcGisPortal = await ArcGISPortal.CreateAsync(_model.DefaultServerUri);
			var item = await ArcGISPortalItem.CreateAsync(arcGisPortal, _currentOfflineMapItem.WebMapId);
			var webMap = await WebMap.FromPortalItemAsync(item);
			foreach (var webMapItem in webMap.OperationalLayers)
			{
				if (webMapItem.Title.Contains(featureLayerInfo.FeatureLayer.ID))
				{
					var uri = new Uri(webMapItem.Url);
					var newSegments = uri.Segments.Take(uri.Segments.Length - 1).ToArray();
					newSegments[newSegments.Length - 1] = newSegments[newSegments.Length - 1].TrimEnd('/');
					var uriBuilder = new UriBuilder(uri) {Path = string.Concat(newSegments)};

					return uriBuilder.Uri;
				}
			}
			return null;
		}

		// function to submit a sync task 
		// -the url for the feature service and the path to the local geodatabase are passed in
		public async Task SyncronizeEditsAsync(FeatureLayerInfo featureLayerInfo, Uri uri)
		{
			// create sync parameters
			var taskParameters = new SyncGeodatabaseParameters
			{
				RollbackOnFailure = true,
				SyncDirection = SyncDirection.Bidirectional
			};

			// cancel if an earlier call was made and hasn't completed
			if (_syncCancellationTokenSource != null)
			{
				_syncCancellationTokenSource.Cancel();
			}

			// create a new cancellation token
			_syncCancellationTokenSource = new CancellationTokenSource();
			var cancelToken = _syncCancellationTokenSource.Token;


			// create a sync task with the url of the feature service to sync
			var syncTask = new GeodatabaseSyncTask(uri);

			// create a new Progress object to report updates to the sync status
			var progress = new Progress<GeodatabaseStatusInfo>();
			progress.ProgressChanged += (sender, s) => { _model.SetMessageInfo("Progress: " + s.Status.ToString()); };

			// call SyncGeodatabaseAsync and pass in: sync params, local geodatabase, completion callback, update interval, progress, and cancellation token

			var geodatabaseFeatureTable = featureLayerInfo.FeatureLayer.FeatureTable as GeodatabaseFeatureTable;

			var gdbResult = await syncTask.SyncGeodatabaseAsync(
				taskParameters,
				geodatabaseFeatureTable.Geodatabase,
				(p, q) =>
				{
					// reset the cancellation token source
					_syncCancellationTokenSource = null;

					// if unsuccessful, report the exception and return
					if (q != null)
					{
						_model.SetMessageInfo("An exception occured: " + q.Message);
						return;
					}

					// if successful, notify the user
					_model.SetMessageInfo("--- Sync completed ---");

					//// optionally, do something with the result
					//var resultUri = p.ResultUri;
					// ...

					UpdateToc(featureLayerInfo);
					_model.SetSyncState(CheckSyncState(false));
				},
				null,
				new TimeSpan(0, 0, 10),
				progress,
				cancelToken);
		}

		#endregion
	}
}