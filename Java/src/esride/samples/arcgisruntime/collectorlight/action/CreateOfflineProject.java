package esride.samples.arcgisruntime.collectorlight.action;

import java.awt.event.ActionEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.InputStreamReader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;
import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.imageio.ImageIO;
import javax.swing.AbstractAction;
import javax.swing.JOptionPane;

import org.apache.commons.lang.ArrayUtils;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpUriRequest;
import org.apache.http.impl.client.DefaultHttpClient;

import com.esri.client.local.ArcGISLocalTiledLayer;
import com.esri.core.geodatabase.GeodatabaseFeatureServiceTable;
import com.esri.core.geodatabase.GeodatabaseFeatureTable;
import com.esri.core.geometry.Envelope;
import com.esri.core.io.UserCredentials;
import com.esri.core.map.CallbackListener;
import com.esri.core.portal.Portal;
import com.esri.core.tasks.ags.geoprocessing.GPMessage;
import com.esri.core.tasks.geodatabase.GenerateGeodatabaseParameters;
import com.esri.core.tasks.geodatabase.GeodatabaseStatusCallback;
import com.esri.core.tasks.geodatabase.GeodatabaseStatusInfo;
import com.esri.core.tasks.geodatabase.GeodatabaseSyncTask;
import com.esri.core.tasks.geodatabase.SyncModel;
import com.esri.core.tasks.tilecache.ExportTileCacheParameters;
import com.esri.core.tasks.tilecache.ExportTileCacheStatus;
import com.esri.core.tasks.tilecache.ExportTileCacheTask;
import com.esri.map.ArcGISTiledMapServiceLayer;
import com.esri.map.JMap;
import com.esri.map.Layer;
import com.esri.map.MapOverlay;

import esride.samples.arcgisruntime.collectorlight.model.SubProject;
import esride.samples.arcgisruntime.collectorlight.services.MessageManager;
import esride.samples.arcgisruntime.collectorlight.services.PortalConnector;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;
import esride.samples.arcgisruntime.collectorlight.ui.SelectExtentOverlay;

public class CreateOfflineProject extends AbstractAction implements PropertyChangeListener {
	
	private static final long serialVersionUID = 1L;
	private static Logger logger = Logger.getLogger(CreateOfflineProject.class.getName());
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	
	private UserCredentials cred;
	private boolean tilesReady;
	private boolean featuresReady;
	private SubProject currentProject;
	private JMap map;
	String projectName;
	int minLevel;
	int maxLevel;

	public CreateOfflineProject() {
		super();
		ProjectManager.addListener(this);
	}
	
	@Override
	public boolean isEnabled() {
		if (currentProject == null || currentProject.isOffline() || !currentProject.isSyncEnabled() || map == null)
			return false;
		return true;
	}
	
	public void setMap(JMap map) {
		this.map = map;
		setEnabled(isEnabled());
	}
	
	public void setProjectName(String name) {
		this.projectName = name;
	}
	
	public void setLevel(int minLevel, int maxLevel) {
		this.minLevel = minLevel;
		this.maxLevel = maxLevel;
	}
	
	
	@Override
	public void actionPerformed(ActionEvent e) {
		
		Runnable runnable = new Runnable() {
			
			@Override
			public void run() {
		
				setEnabled(false);
				cred = PortalConnector.getUser(true);
				if (cred == null) {
					setEnabled(true);
					return;
				}
				SubProject newProject = createProject();
				if (newProject == null) {
					setEnabled(true);
					return;
				}
				tilesReady = false;
				featuresReady = false;
				String path = ProjectManager.getPathForOfflineSubProject(newProject.getParentProject().getId(), newProject.getName());
				createMetaData(newProject, path);
				Envelope extent = map.getExtent();
				for (MapOverlay overlay : map.getMapOverlays()) {
					if (overlay instanceof SelectExtentOverlay) {
						Envelope overlayExtent = ((SelectExtentOverlay)overlay).getExtent();
						if (overlayExtent != null) {
							extent = overlayExtent;
						}
					}
				}
				createTileCache(path, newProject, extent);
				createGeodatabaseFromService(path, newProject, extent);
			}
		};
		Thread t = new Thread(runnable);
		t.start();
		
	}
	
	private SubProject createProject () {
		
		if (projectName == null) {
			return null;
		}
		String projectPath = ProjectManager.getPathForOfflineSubProject(currentProject.getParentProject().getId(), projectName);
		File f = new File(projectPath); 
		if (f.exists()) {
			String mes = String.format(res.getString("CreateAction.FileAlreadyExist"), projectPath);
			JOptionPane.showMessageDialog(null, mes, "Error", JOptionPane.ERROR_MESSAGE);
			return null;
		}
		return new SubProject(currentProject.getParentProject(), projectName, true);
	}
	
	private void createMetaData(SubProject newProject, String path) {
		try {
			Path parentPath = Paths.get(path).getParent();
			if (!Files.exists(parentPath)) {
				Files.createDirectory(parentPath);
			}
			Path iconParentPath = Paths.get(parentPath + "/icon.png");
			if (!Files.exists(iconParentPath)) {
				Files.createFile(iconParentPath);
				ByteArrayOutputStream baos = new ByteArrayOutputStream();
		        ImageIO.write(newProject.getParentProject().getIcon(), "PNG", baos);
				Files.write(iconParentPath, baos.toByteArray());
			}
			Path iconPath = Paths.get(path + "/icon.png");
			Path projectPath = Paths.get(path);
			if (!Files.exists(iconPath)) {
				if (!Files.exists(projectPath)) {
					Files.createDirectory(projectPath);
				}
				Files.createFile(iconPath);
				ByteArrayOutputStream baos = new ByteArrayOutputStream();
		        ImageIO.write(newProject.getParentProject().getIcon(), "PNG", baos);
				Files.write(iconPath, baos.toByteArray());
			}
			Portal portal = PortalConnector.getPortal(true);
			
			Path webmapJsonPath = Paths.get(parentPath + "/webmapitem.json");
			if (!Files.exists(webmapJsonPath)) {
				HttpClient client = new DefaultHttpClient();
			    HttpUriRequest request = new HttpGet(portal.findSharingUrl() + "/content/items/" +  newProject.getParentProject().getId() + "/data?f=json&token=" + PortalConnector.getUser(true).getToken());
				HttpResponse response = client.execute(request);
				BufferedReader rd = new BufferedReader (new InputStreamReader(response.getEntity().getContent()));
				String line = "";
				while ((line = rd.readLine()) != null) {
					Files.write(webmapJsonPath, line.getBytes());
				}
			}
			
			Path portalJsonPath = Paths.get(parentPath + "/portalitem.json");
			if (!Files.exists(portalJsonPath)) {
				HttpClient client = new DefaultHttpClient();
			    HttpUriRequest request = new HttpGet(portal.findSharingUrl() + "/content/items/" +  newProject.getParentProject().getId() + "?f=json&token=" + PortalConnector.getUser(true).getToken());
				HttpResponse response = client.execute(request);
				BufferedReader rd = new BufferedReader (new InputStreamReader(response.getEntity().getContent()));
				String line = "";
				while ((line = rd.readLine()) != null) {
					Files.write(portalJsonPath, line.getBytes());
				}
			}
			
		} catch (Exception e) {
			logger.log(Level.SEVERE, "Metadaten konnten nicht erzeugt werden", e);
		}
	}
	
	private ExportTileCacheParameters createTileParameters(Envelope extent) {
		
		ExportTileCacheParameters params = new ExportTileCacheParameters(
			true,  
			minLevel,
			maxLevel,
		    extent,
		    map.getSpatialReference());
		return params;
	}

	private void createTileCache(String projectPath, final SubProject newProject, Envelope extent) {
		
		Layer layer = currentProject.getTiledLayer();
		if (layer == null || !(layer instanceof ArcGISTiledMapServiceLayer)) {
			logger.warning("Download der Basemap nicht möglich");
		}
		String url = layer.getUrl();
		String exportUrl = url.replaceFirst("services.arcgisonline.com", "tiledbasemaps.arcgis.com");
		String basemapName = "/offline.tpk";
		String[] splittedUrl = exportUrl.split("/");
		for (int i = splittedUrl.length - 1; i >= 1; i--) {
			String string = splittedUrl[i];
			if ("MapServer".equals(string)) {
				basemapName = "/" + splittedUrl[i - 1] + ".tpk";
				break;
			}
		}
		
		ExportTileCacheTask tileCacheTask = new ExportTileCacheTask(exportUrl, cred);
		ExportTileCacheParameters params = createTileParameters(extent);
		MessageManager.setProgressMessage(res.getString("CreateAction.StartTiles"));
		MessageManager.showProgress();
		
		tileCacheTask.generateTileCache( 
		     
			 // parameters 
		     params, 
		     
		     // status callback 
		     new CallbackListener<ExportTileCacheStatus>() { 
		    	 int prevMesCount = 0;
		        
			     @Override 
			     public void onError(Throwable e) {
			    	 MessageManager.addTextMessage("TILES: " + e.getMessage());
			    	 MessageManager.hideProgress();
				     JOptionPane.showMessageDialog(null, res.getString("CreateAction.TileError"), "", JOptionPane.ERROR_MESSAGE);
			         logger.log(Level.SEVERE, "Tiles konnten nicht erzeugt werden", e);
			     }
			     
			     @Override 
		         public void onCallback(ExportTileCacheStatus status) {
			    	 MessageManager.setProgressMessage(res.getString("CreateAction.StatusTiles") + status.getStatus());
		    		 GPMessage[] mesList = status.getJobResource().getMessages();
		    		 for (int i = prevMesCount; i < mesList.length; i++) {
		    			 MessageManager.addTextMessage("TILES: " + mesList[i].getDescription());
					 }
		    		 prevMesCount = mesList.length;
			     }
			 },
			 
		     // callback when download complete 
		     new CallbackListener<String>() {
		  
			     @Override 
			     public void onError(Throwable e) {
			    	 MessageManager.addTextMessage("TILES: " + e.getMessage());
			    	 MessageManager.hideProgress();
				     JOptionPane.showMessageDialog(null, res.getString("CreateAction.TileError"), "", JOptionPane.ERROR_MESSAGE);
				     logger.log(Level.SEVERE, "Tiles konnten nicht erzeugt werden", e);
			     } 
		  
			     @Override 
			     public void onCallback(String path) { 
			    	 if (path != null && path.trim().length() > 0) {
			    		 MessageManager.setProgressMessage(res.getString("CreateAction.DownloadTilesComplete"));
			    		 MessageManager.addTextMessage(res.getString("CreateAction.DownloadTilesComplete"));
				    	 final ArcGISLocalTiledLayer layer = new ArcGISLocalTiledLayer(path);
				    	 layer.setName("Created Offline - " + path);
				    	 if (featuresReady) {
				    		 MessageManager.addTextMessage(res.getString("CreateAction.TilesReady"));
				    		 MessageManager.hideProgress();
				    		 ProjectManager.addNewSubToCurrentProject(newProject.getParentProject(), newProject.getName());
				    		 setEnabled(true);
						 }
				    	 tilesReady = true;
			    	 }
		       } 
		     },
		     
		     // path for downloaded tile cache 
		     projectPath + basemapName);
	}
	
	private void createGeodatabaseFromService(final String pathToGdb, final SubProject newProject, Envelope extent) {
		
		List<GeodatabaseFeatureTable> featureTableList = currentProject.getFeatureTableList();
		if (featureTableList == null || featureTableList.size() <= 0) {
			return;
		}
		
		GeodatabaseFeatureServiceTable table = (GeodatabaseFeatureServiceTable)featureTableList.get(0);
		int lastIndexOf = table.getServiceUrl().lastIndexOf("/");
		String featureServiceUrl = table.getServiceUrl().substring(0, lastIndexOf);

		List<Integer> layerIds = new ArrayList<Integer>();
		for (GeodatabaseFeatureTable featureTable : featureTableList) {
			GeodatabaseFeatureServiceTable featureServiceTable = (GeodatabaseFeatureServiceTable)featureTable;
			int indexId = featureServiceTable.getServiceUrl().lastIndexOf("/");
			if (featureServiceUrl.equals(featureServiceTable.getServiceUrl().substring(0, indexId))) {
				layerIds.add(featureTable.getFeatureServiceLayerId());
			}
		}
		
		//set up the parameters
		GenerateGeodatabaseParameters params = new GenerateGeodatabaseParameters(
				ArrayUtils.toPrimitive(layerIds.toArray(new Integer[layerIds.size()])),
				extent,
				map.getSpatialReference(),
				false,  
				SyncModel.GEODATABASE,
				map.getSpatialReference());
		String serviceName = "/offlineFeatures.geodatabase";
		String[] splittedUrl = featureServiceUrl.split("/");
		for (int i = splittedUrl.length - 1; i >= 1; i--) {
			String string = splittedUrl[i];
			if ("FeatureServer".equals(string)) {
				serviceName = "/" + splittedUrl[i - 1] + ".geodatabase";
				break;
			}
		}
		
		// Status callback
		GeodatabaseStatusCallback statusCallback = new GeodatabaseStatusCallback() {
			@Override 
		    public void statusUpdated(GeodatabaseStatusInfo status) {
				MessageManager.setProgressMessage(res.getString("CreateAction.StatusGdb") + status.getStatus());
				MessageManager.addTextMessage(res.getString("CreateAction.StatusGdb") + status.getStatus());
				if (status.getError() != null) {
					MessageManager.addTextMessage(res.getString("CreateAction.GdbErrorCode") + status.getError().getCode());
					MessageManager.addTextMessage(res.getString("CreateAction.GdbErrorDescription") + status.getError().getDescription());
					MessageManager.addTextMessage(res.getString("CreateAction.GdbErrorMessage") + status.getError().getThrowable().getMessage());
				}
			} 
		}; 

		//Complete callback 
		CallbackListener<String> gdbResponseCallback = new CallbackListener<String>() {
			@Override 
		    public void onError(Throwable e) {
				MessageManager.addTextMessage("GDB: " + e.getMessage());
		    	MessageManager.hideProgress();
	    	    JOptionPane.showMessageDialog(null, res.getString("CreateAction.GdbError"), "", JOptionPane.ERROR_MESSAGE);
	    	    logger.log(Level.SEVERE, "GDB konnte nicht erzeugt werden", e);
			    setEnabled(true);
		    }
			
			@Override 
			public void onCallback(String geodatabase) {
				MessageManager.setProgressMessage(res.getString("CreateAction.DownloadGdbComplete"));
				MessageManager.addTextMessage(res.getString("CreateAction.DownloadGdbComplete"));
				// now use the path to downloaded geodatabase to create a GdbFeatureTable and FeatureLayer 			
				if (tilesReady) {
					MessageManager.addTextMessage(res.getString("CreateAction.GdbReady"));
					MessageManager.hideProgress();
					ProjectManager.addNewSubToCurrentProject(newProject.getParentProject(), newProject.getName()); 
					setEnabled(true);
				 }
				featuresReady = true;
			}
		}; 

		// ------------------------------------------------------------------------ 
		// Generate the geodatabase from the service and download 
		// ------------------------------------------------------------------------ 
		GeodatabaseSyncTask gdbTask = new GeodatabaseSyncTask(featureServiceUrl, PortalConnector.getUser(true));
		
		gdbTask.generateGeodatabase(
				params, pathToGdb + serviceName, false, statusCallback, gdbResponseCallback);
		MessageManager.setProgressMessage(res.getString("CreateAction.StartGdb"));
		MessageManager.showProgress();
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		if (evt.getPropertyName().equals(ProjectManager.PROP_SUB_CHANGED)) {
			currentProject = (SubProject) evt.getNewValue();
			setEnabled(isEnabled());
		}
		
	}

}
