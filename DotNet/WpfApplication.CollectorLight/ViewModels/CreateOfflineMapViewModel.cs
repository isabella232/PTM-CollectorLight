using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.WebMap;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication.CollectorLight.BusinessLogic;
using WpfApplication.CollectorLight.DomainModels;
using WpfApplication.CollectorLight.DomainModels.Json;

namespace WpfApplication.CollectorLight.ViewModels
{
	public class CreateOfflineMapViewModel : ViewModelBase
	{
		private const string ExportTiledBaseMapServiceHost = "tiledbasemaps.arcgis.com";
		private const string PortalItemJsonFileName = "portalitem.json";
		private const string WebMapItemJsonFileName = "webmapitem.json";
		private const string ThumbnailFileName = "icon.png";
		private readonly IModel _model;
		private readonly ResourceDictionary _resourceDictionary;

		private Graphic _areaToExportGraphic;
		private ArcGisPortalWebMapItem _currentArcGisPortalWebMapItem;
		private MapView _currentEsriMapView;
		private string _exportMapPath = string.Empty;
		private ExportTileCacheTask _exportTileCacheTask;
		private GenerateTileCacheParameters _generateTileCacheParameters;
		private GraphicsOverlay _graphicsOverlay;
		private bool _isExportReady;
		private bool _isFocused;
		private bool _isMapLoaded;
		private string _offlineMapName;

		public CreateOfflineMapViewModel(IModel model)
		{
			_model = model;
			InitializeModelEvents();
			InitializeRelayCommands();

			_resourceDictionary = new ResourceDictionary
			{
				Source =
					new Uri("/WpfApplication.CollectorLight;component/Dictionary.xaml",
						UriKind.RelativeOrAbsolute)
			};
		}

		public RelayCommand MarkAreaCommand { get; private set; }
		public RelayCommand ExportMapCommand { get; private set; }
		public RelayCommand ChangeFocusCommand { get; private set; }
		public RelayCommand CancelCommand { get; private set; }

		public CreateOfflineMapState CurrentState { get; set; }

		public MapView CurrentEsriMapView
		{
			get { return _currentEsriMapView; }
			set
			{
				_currentEsriMapView = value;
				RaisePropertyChanged("CurrentEsriMapView");
			}
		}

		public string OfflineMapName
		{
			get { return _offlineMapName; }
			set
			{
				_offlineMapName = value;
				RaisePropertyChanged("OfflineMapName");
				IsExportReady = CheckIsExportReady();
			}
		}

		public bool IsFocused
		{
			get { return _isFocused; }
			set
			{
				_isFocused = value;
				RaisePropertyChanged("IsFocused");
			}
		}

		public bool IsExportReady
		{
			get { return _isExportReady; }
			set
			{
				_isExportReady = value;
				RaisePropertyChanged("IsExportReady");
			}
		}

		public bool IsMapLoaded
		{
			get { return _isMapLoaded; }
			set
			{
				_isMapLoaded = value;
				RaisePropertyChanged("IsMapLoaded");
			}
		}

		private void InitializeModelEvents()
		{
			_model.CreateOfflineMap += (p, q) =>
			{
				if (CurrentState == CreateOfflineMapState.GeneratingOfflineMap)
				{
					return;
				}

				Reset();
				CurrentState = CreateOfflineMapState.IsActive;
				LoadMap(p, q);
				_model.TriggerChangeViewEvent(View.CreateOfflineMapView, View.CreateOfflineMapView, false);
			};
		}

		private void InitializeRelayCommands()
		{
			MarkAreaCommand = new RelayCommand(() =>
			{
				IsExportReady = false;
				AddGraphicsAsync();
			});

			ExportMapCommand = new RelayCommand(() => ExportMap());

			ChangeFocusCommand = new RelayCommand(() =>
			{
				IsFocused = true;
				CheckIsExportReady();
			});

			CancelCommand = new RelayCommand(() =>
			{
				_model.TriggerChangeViewEvent(View.ArcGisPortalWebMapItemsView, View.CreateOfflineMapView, false);
				Reset();
			});
		}

		private async Task StoreWebmapItemJson(string offlineWebMapItemRootPath)
		{
			await Task.Run(() =>
			{
				string json = null;
				try
				{
					json = WebMapItemJsonConverter.GetWebMapJsonString(_currentArcGisPortalWebMapItem.WebMapItemJsonUri);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString(), "Der Download der WebMap Metadaten hat leider nicht funktioniert.");
				}
				using (var outfile = new StreamWriter(Path.Combine(offlineWebMapItemRootPath, WebMapItemJsonFileName)))
				{
					outfile.Write(json);
				}
			});
		}

		private async Task StorePortalItemJson(string offlineWebMapItemRootPath)
		{
			await Task.Run(() =>
			{
				//Store json
				var json = PortalItemJsonConverter.GetPortalItemJsonString(_currentArcGisPortalWebMapItem.PortalItemJsonUri);
				using (var outfile = new StreamWriter(Path.Combine(offlineWebMapItemRootPath, PortalItemJsonFileName)))
				{
					outfile.Write(json);
				}
			});
		}

		private async Task StorePortalItemThumbnail(string offlineWebMapItemRootPath)
		{
			await Task.Run(() =>
			{
				try
				{
					//Das Icon auf PortalItem Ebene wird für die .NET Anwendung eigentlich nicht gebraucht. Wir laden es 
					//herunter, um die Austauschbarkeit der Caches zwischen den verschiedenen Anwendungen zu gewährleisten
					using (var webClient = new WebClient())
					{
						webClient.DownloadFileAsync(_currentArcGisPortalWebMapItem.ArcGisPortalItem.ThumbnailUri,
							Path.Combine(offlineWebMapItemRootPath, ThumbnailFileName));
					}
				}
				catch
				{
				}
			});
		}

		private string CreateOfflineWebMapDirectory(string offlineWebMapItemRootPath)
		{
			var path = GetOfflineWebMapItemPath(offlineWebMapItemRootPath);
			Directory.CreateDirectory(path);
			return path;
		}

		private void CreateThumbnail()
		{
			var thumbnail = GetPngImage(CurrentEsriMapView, 0.5);
			var fileStream = new FileStream(string.Format("{0}/{1}", _exportMapPath, OfflineMapItem.ThumbnailName),
				FileMode.Create, FileAccess.ReadWrite);
			var binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(thumbnail);
			binaryWriter.Close();
		}

		private static byte[] GetPngImage(UIElement source, double scale)
		{
			var actualHeight = source.RenderSize.Height;
			var actualWidth = source.RenderSize.Width;

			var renderHeight = actualHeight*scale;
			var renderWidth = actualWidth*scale;

			var renderTarget = new RenderTargetBitmap((int) renderWidth, (int) renderHeight, 96, 96, PixelFormats.Pbgra32);
			var sourceBrush = new VisualBrush(source);

			var drawingVisual = new DrawingVisual();
			var drawingContext = drawingVisual.RenderOpen();

			using (drawingContext)
			{
				drawingContext.PushTransform(new ScaleTransform(scale, scale));
				drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
			}
			renderTarget.Render(drawingVisual);

			var pngEncoder = new PngBitmapEncoder();
			pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

			Byte[] _imageArray;

			using (var outputStream = new MemoryStream())
			{
				pngEncoder.Save(outputStream);
				_imageArray = outputStream.ToArray();
			}

			return _imageArray;
		}

		private static string GetServiceNameByUrl(Uri url)
		{
			int index;
			int dummy;
			if (int.TryParse(url.Segments[url.Segments.Length - 1], out dummy))
			{
				index = url.Segments.Length - 3;
			}
			else
			{
				index = url.Segments.Length - 2;
			}
			return url.Segments[index].Replace("/", string.Empty);
		}

		private string GetOfflineWebMapItemRootPath()
		{
			var path = string.Empty;
			path += string.Format(_model.GetOfflineMapItemsPath().AbsolutePath);
			path += string.Format("\\{0}", _currentArcGisPortalWebMapItem.ArcGisPortalItem.Id);

			return path;
		}

		private string GetOfflineWebMapItemPath(string offlineWebMapItemRootPath)
		{
			var path = offlineWebMapItemRootPath;

			if (!Directory.Exists(path))
			{
				path += string.Format("\\{0}", OfflineMapName);
				return Path.GetFullPath(path);
			}

			if (!Directory.Exists(string.Format("{0}\\{1}", path, OfflineMapName)))
			{
				path += string.Format("\\{0}", OfflineMapName);
				return Path.GetFullPath(path);
			}

			var directories = Directory.GetDirectories(path);

			int highestNumber = 0;
			foreach (var directory in directories)
			{
				var fullOfflineMapName = directory.Split('\\').Last();
				var orgOfflineMapName = fullOfflineMapName.Split('_').First();

				if (!String.Equals(orgOfflineMapName, OfflineMapName, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				int number;
				if (!int.TryParse(directory.Split('_').Last(), out number)) continue;
				if (number > highestNumber)
				{
					highestNumber = number;
				}
			}
			highestNumber++;
			path += string.Format("\\{0}_{1}", OfflineMapName, highestNumber);
			return Path.GetFullPath(path);
		}

		private async void LoadMap(ArcGisPortalWebMapItem arcGisPortalWebMapItem, ArcGISPortal arcGisPortal)
		{
			_currentArcGisPortalWebMapItem = arcGisPortalWebMapItem;
			_graphicsOverlay = new GraphicsOverlay();
			_graphicsOverlay.Renderer = new SimpleRenderer();

			var vm = await WebMapViewModel.LoadAsync(arcGisPortalWebMapItem.WebMap, arcGisPortal);
			var mapView = new MapView {Map = vm.Map};
			mapView.GraphicsOverlays.Add(_graphicsOverlay);
			CurrentEsriMapView = mapView;

			IsMapLoaded = true;
		}

		private async void AddGraphicsAsync()
		{
			if (CurrentEsriMapView.Editor.IsActive)
				CurrentEsriMapView.Editor.Cancel.Execute(null);

			_graphicsOverlay.Graphics.Clear();
			_areaToExportGraphic = null;
			_areaToExportGraphic = await CreateGraphicAsync();


			IsExportReady = CheckIsExportReady();
		}

		private bool CheckIsExportReady()
		{
			var regex = new Regex(@"[a-zA-Z0-9]");

			return _areaToExportGraphic != null && !string.IsNullOrEmpty(OfflineMapName) && regex.IsMatch(OfflineMapName);
		}

		// Draw and add a single graphic to the graphic layer
		private async Task<Graphic> CreateGraphicAsync()
		{
			try
			{
				var symbol = _resourceDictionary["RedFillSymbol"] as Symbol;

				// wait for user to draw the shape
				var geometry = await CurrentEsriMapView.Editor.RequestShapeAsync(DrawShape.Rectangle, symbol);

				// add the new graphic to the graphic layer
				var graphic = new Graphic(geometry, symbol);

				_graphicsOverlay.Graphics.Add(graphic);

				return graphic;
			}
			catch (TaskCanceledException)
			{
				_graphicsOverlay.Graphics.Clear();
				_areaToExportGraphic = null;
			}
			catch (Exception ex)
			{
				_model.SetMessageInfo("Error drawing graphic: " + ex.Message);
			}
			return null;
		}

		private void Reset()
		{
			if (_graphicsOverlay != null)
			{
				_graphicsOverlay.Graphics.Clear();
			}
			CurrentState = CreateOfflineMapState.IsReady;
			_exportTileCacheTask = null;
			_generateTileCacheParameters = null;
			_exportMapPath = string.Empty;
			_areaToExportGraphic = null;
			OfflineMapName = string.Empty;
			CurrentEsriMapView = null;
			IsMapLoaded = false;
		}

		#region Export BaseMap

		private async Task ExportMap()
		{
			_model.TriggerProgressChangedEvent(_exportMapPath,
				new ProgressStateItem(string.Format("Start Export {0}: ", _offlineMapName), true, 0));

			CurrentState = CreateOfflineMapState.GeneratingOfflineMap;
			_model.SetMessageInfo(string.Format(">>> Start map export ... '{0}' ", _offlineMapName));

			var offlineWebMapItemRootPath = GetOfflineWebMapItemRootPath();

			if (!Directory.Exists(offlineWebMapItemRootPath))
			{
				try
				{
					Directory.CreateDirectory(offlineWebMapItemRootPath);
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						string.Format("Der Ordner '{0}' konnte nicht angelegt werden. Bitte legen Sie den Ordner manuell an.\n{1}",
							offlineWebMapItemRootPath, ex));
					throw;
				}
			}
			//In den Root
			//Json Informationen ablegen
			_model.SetMessageInfo("Storing Meta Information ...");
			await Task.WhenAll(
				StoreWebmapItemJson(offlineWebMapItemRootPath),
				StorePortalItemJson(offlineWebMapItemRootPath),
				StorePortalItemThumbnail(offlineWebMapItemRootPath)
				);
			_model.SetMessageInfo("Storing Json completed");

			_exportMapPath = CreateOfflineWebMapDirectory(offlineWebMapItemRootPath);
			_model.SetMessageInfo(string.Format("Folder created: {0}", _exportMapPath));

			CreateThumbnail();
			_model.SetMessageInfo("Thumbnail created");

			_model.TriggerRefreshOfflineMapItemsEvent();
			await CurrentEsriMapView.SetViewAsync(_areaToExportGraphic.Geometry.Extent);
			_model.TriggerRefreshOfflineMapItemsEvent();
			_model.TriggerChangeViewEvent(View.OfflineMapItemsView, View.CreateOfflineMapView, false);

			//synchron, remove 'await' for asyncron pattern
			_model.SetMessageInfo("Exporting BaseMap and Vector Data (in parallel)...");
			_model.TriggerProgressChangedEvent(_exportMapPath,
				new ProgressStateItem("Exporting BaseMap / Vector Data...", true, 0));

			await Task.WhenAll(
				DoExportBaseMapDataAsync(),
				DoExportVectorDataAsync()
				);
			_model.SetMessageInfo("BaseMap / Vector Data Export completed");
			_model.TriggerProgressChangedEvent(_exportMapPath, new ProgressStateItem("Export complete", true, 0));

			_model.SetMessageInfo(">>>Map export complete!");
			CurrentState = CreateOfflineMapState.IsReady;
			_model.TriggerProgressChangedEvent(_exportMapPath, new ProgressStateItem());
		}

		private async Task DoExportBaseMapDataAsync()
		{
			_model.SetMessageInfo("Estimating TileCacheSize ...");
			await EstimateTileCacheSizeAsync();
			await ExportBaseMapData();
		}

		private async Task EstimateTileCacheSizeAsync()
		{
			if (!(CurrentEsriMapView.Map.Layers[0] is ArcGISTiledMapServiceLayer))
			{
				return;
			}

			var arcGisTiledMapServiceLayer = CurrentEsriMapView.Map.Layers[0] as ArcGISTiledMapServiceLayer;

			_generateTileCacheParameters = new GenerateTileCacheParameters
			{
				Format = ExportTileCacheFormat.TilePackage,
				MinScale = CurrentEsriMapView.MinScale,
				MaxScale = CurrentEsriMapView.MaxScale,
				GeometryFilter = _areaToExportGraphic.Geometry.Extent
			};

			Uri exportArcGisTiledMapServiceLayerServiceUri = null;
			if (arcGisTiledMapServiceLayer.ServiceUri.Contains("services.arcgisonline.com"))
			{
				var uriBuilder = new UriBuilder(arcGisTiledMapServiceLayer.ServiceUri)
				{
					Scheme = "https",
					Host = ExportTiledBaseMapServiceHost,
					Port = -1
				};
				exportArcGisTiledMapServiceLayerServiceUri = uriBuilder.Uri;
			}
			else
			{
				exportArcGisTiledMapServiceLayerServiceUri = new Uri(arcGisTiledMapServiceLayer.ServiceUri);
			}


			_exportTileCacheTask = new ExportTileCacheTask(exportArcGisTiledMapServiceLayerServiceUri);

			var exportTileCacheJobStatusProgress = new Progress<ExportTileCacheJob>();
			exportTileCacheJobStatusProgress.ProgressChanged += (p, q) =>
			{
				if (q.Messages == null)
					return;

				var text = string.Format("Job Status: {0}\n\nMessages:\n=====================\n", q.Status);
				foreach (GPMessage message in q.Messages)
				{
					text += string.Format("Message type: {0}\nMessage: {1}\n--------------------\n",
						message.MessageType, message.Description);
				}
			};

			var tcs = new TaskCompletionSource<EstimateTileCacheSizeResult>();
			await _exportTileCacheTask.EstimateTileCacheSizeAsync(_generateTileCacheParameters,
				(result, ex) => // Callback for when estimate operation has completed
				{
					if (result != null)
					{
						tcs.SetResult(result);
					}
					if (ex != null)
					{
						tcs.SetException(ex);
					}
					if (ex == null) // Check whether operation completed with errors
					{
						_model.SetMessageInfo(string.Format("Tiles: {0} - Size (kb): {1:0}", result.TileCount, result.Size/1024));
					}
					else
					{
						_model.SetMessageInfo(string.Format("Error: {0}", ex.Message));
					}
				}, TimeSpan.FromSeconds(5), CancellationToken.None, exportTileCacheJobStatusProgress);

			await tcs.Task;
		}

		private async Task ExportBaseMapData()
		{
			var downloadOptions = new DownloadTileCacheParameters(_exportMapPath)
			{
				OverwriteExistingFiles = true
			};

			var generateStatusCheckProgress = new Progress<ExportTileCacheJob>();
			generateStatusCheckProgress.ProgressChanged += (p, q) =>
			{
				if (q.Messages.Count < 1)
					return;
				_model.SetMessageInfo(string.Format("{0}", q.Messages[1].Description));
			};

			var downloadProgressChanged = new Progress<ExportTileCacheDownloadProgress>();
			downloadProgressChanged.ProgressChanged += (p, q) =>
			{
				//_model.SetMessageInfo(string.Format("Downloading file...\n{0:P0} complete\n" +
				//"Bytes read: {1}", q.ProgressPercentage, q.CurrentFileBytesReceived));
			};


			var downloadTileCacheResult = await _exportTileCacheTask.GenerateTileCacheAndDownloadAsync(
				_generateTileCacheParameters,
				downloadOptions, TimeSpan.FromSeconds(5), CancellationToken.None, generateStatusCheckProgress,
				downloadProgressChanged);
			_model.SetMessageInfo("Downloading file...");
			var arcGisTiledMapServiceLayer = CurrentEsriMapView.Map.Layers[0] as ArcGISTiledMapServiceLayer;
			var newName = GetServiceNameByUrl(new Uri(arcGisTiledMapServiceLayer.ServiceUri));
			var renameTpkPath = downloadTileCacheResult.OutputPath.Replace(arcGisTiledMapServiceLayer.ServiceInfo.MapName,
				newName);
			File.Move(downloadTileCacheResult.OutputPath, renameTpkPath);
			_model.SetMessageInfo(string.Format("Download completed: {0}", renameTpkPath));
		}

		#endregion

		#region Export Vector Data

		private async Task DoExportVectorDataAsync()
		{
			var featureLayerList = CurrentEsriMapView.Map.Layers.OfType<FeatureLayer>().Select(layer => layer).ToList();

			_model.SetMessageInfo("Exporting Features ....");
			await DoExportGeodataBase(featureLayerList);
		}

		private async Task DoExportGeodataBase(List<FeatureLayer> featureLayerList)
		{
			try
			{
				Uri featureServerUri = null;
				var layerIdList = new List<int>();
				foreach (var featureLayer in featureLayerList)
				{
					var geodatabaseFeatureServiceTable = featureLayer.FeatureTable as ServiceFeatureTable;
					if (geodatabaseFeatureServiceTable == null)
						return;

					var featureServiceUri = new Uri(geodatabaseFeatureServiceTable.ServiceUri);

					if (featureServerUri == null)
					{
						var newSegments = featureServiceUri.Segments.Take(featureServiceUri.Segments.Length - 1).ToArray();
						newSegments[newSegments.Length - 1] = newSegments[newSegments.Length - 1].TrimEnd('/');

						var uriBuilder = new UriBuilder(featureServiceUri) {Path = string.Concat(newSegments)};
						featureServerUri = uriBuilder.Uri;
					}

					if (featureServiceUri.ToString().Contains(featureServerUri.ToString()))
					{
						var layerId = int.Parse(featureServiceUri.Segments.Last());
						layerIdList.Add(layerId);
					}
				}

				_model.SetMessageInfo("Creating GeodatabaseSyncTask...");
				var geodatabaseSyncTask = new GeodatabaseSyncTask(featureServerUri);

				var generateGeodatabaseParameters = new GenerateGeodatabaseParameters(layerIdList, _areaToExportGraphic.Geometry)
				{
					GeodatabasePrefixName = "EsriDE.Samples", //
					ReturnAttachments = true,
					OutSpatialReference = CurrentEsriMapView.SpatialReference,
					SyncModel = SyncModel.PerGeodatabase
				};


				var taskCompletionSource = new TaskCompletionSource<GeodatabaseStatusInfo>();
				Action<GeodatabaseStatusInfo, Exception> completionAction = (info, ex) =>
				{
					if (ex != null)
						taskCompletionSource.SetException(ex);
					taskCompletionSource.SetResult(info);
				};

				var generationProgress = new Progress<GeodatabaseStatusInfo>();
				generationProgress.ProgressChanged +=
					(sndr, sts) => { _model.SetMessageInfo(string.Format("{0}: {1}", sts.Status.ToString(), sts.LastUpdatedTime)); };

				_model.SetMessageInfo("Starting Generate Geodatabase...");
				var result = await geodatabaseSyncTask.GenerateGeodatabaseAsync(generateGeodatabaseParameters, completionAction,
					TimeSpan.FromSeconds(3), generationProgress, CancellationToken.None);

				_model.SetMessageInfo("Waiting on geodatabase from server...");
				var statusResult = await taskCompletionSource.Task;

				_model.SetMessageInfo("Downloading Geodatabase...");
				var gdbPath = await DownloadGeodatabase(statusResult, GetServiceNameByUrl(featureServerUri));
				_model.SetMessageInfo(string.Format("Download completed : {0}", gdbPath));
			}
			catch (Exception ex)
			{
				_model.SetMessageInfo(ex.Message);
			}
		}

		// Download a generated geodatabase file
		private async Task<string> DownloadGeodatabase(GeodatabaseStatusInfo statusResult, string gdbName)
		{
			var client = new ArcGISHttpClient();
			var gdbStream = client.GetOrPostAsync(statusResult.ResultUri, null);
			var gdbPath =
				Path.GetFullPath(string.Format("{0}/{1}{2}", _exportMapPath, gdbName, OfflineMapItem.GeodatabaseFilenameExtension));


			await Task.Factory.StartNew(async () =>
			{
				using (var stream = File.Create(gdbPath))
				{
					await gdbStream.Result.Content.CopyToAsync(stream);
				}
			});

			return gdbPath;
		}

		#endregion
	}
}