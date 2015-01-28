package esride.samples.arcgisruntime.collectorlight.services;

import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.esri.client.local.ArcGISLocalTiledLayer;
import com.esri.core.geodatabase.Geodatabase;
import com.esri.core.geodatabase.GeodatabaseFeatureServiceTable;
import com.esri.core.portal.WebMap;
import com.esri.core.portal.WebMapLayer;
import com.esri.map.ArcGISTiledMapServiceLayer;

import esride.samples.arcgisruntime.collectorlight.model.MapItem;
import esride.samples.arcgisruntime.collectorlight.model.MapItemList;
import esride.samples.arcgisruntime.collectorlight.model.Project;
import esride.samples.arcgisruntime.collectorlight.model.SubProject;

public class ProjectManager {
	
	public static final String PROP_PROJECT_CHANGED = "SelectedProjectChanged";
	public static final String PROP_SUB_CHANGED = "SelectedSubProjectChanged";
	public static final String PROP_SUB_ADDED = "SubProjectAdded";
	public static final String PROP_SUB_REMOVED = "SubProjectRemoved";
	private static Logger logger = Logger.getLogger(ProjectManager.class.getName());
	private static Project currentProject;
	private static SubProject currentSubProject;
	private static final PropertyChangeSupport support = new PropertyChangeSupport(new ProjectManager());
	
	public static void addListener(PropertyChangeListener listener) {
		support.addPropertyChangeListener(listener);
	}
	
	public static Project getCurrentProject() {
		return currentProject;
	}
	
	public static void openProject(MapItem item, String nameSubProject) {
		logger.info("Open Project mit ID " + item.getId());
		Project project = new Project(item);
		if (item.isOnlineItem()) {
			loadOnlineMap(project, item);
		}
		checkAndLoadOfflineProjects(project, item);
		Project oldProject = currentProject;
		currentProject = project;
		support.firePropertyChange(PROP_PROJECT_CHANGED, oldProject, currentProject);
		switchToSubByName(nameSubProject);
		if (oldProject != null)
			oldProject.dispose();
	}
	
	public static void switchToSubProject(SubProject sub) {
		SubProject oldSub = currentSubProject;
		currentSubProject = sub;
		support.firePropertyChange(PROP_SUB_CHANGED, oldSub, currentSubProject);
	}
	
	public static String getPathForOfflineSubProject(String id, String name) {
		return "data/offlineProjects/" + id + "/" + name;
	}
	
	public static void addNewSubToCurrentProject(Project project, String nameNewSubProject) {
		MapItem offlineItem = MapItemList.getOfflineList().getItemById(project.getId());
		if (offlineItem == null) {
			MapItemList.getOfflineList().addItem(project.getMapItem());
		}
		 
		Path path = Paths.get(getPathForOfflineSubProject(project.getId(), nameNewSubProject));
		try {
			SubProject sub = loadOfflineSubProject(project, path);
			if (sub == null) {
				logger.warning("Subprojekt konnte nicht geladen werden " + nameNewSubProject);
			}
			if (project != currentProject) {
				logger.warning("Current Project wurde verändert -> Add Sub wird ignoriert");
				return;
			}
			currentProject.addSubProject(sub);
			support.firePropertyChange(PROP_SUB_ADDED, null, sub);
		} catch (IOException e) {
			logger.log(Level.SEVERE, "Subprojekt konnte nicht geladen werden " + nameNewSubProject, e);
		}
	}
	
	public static void removeSubFromCurrentProject(Project project, String nameSubToRemove) {
		if (project != currentProject) {
			logger.warning("Current Project wurde verändert -> Remove Sub wird ignoriert");
			return;
		}
		SubProject subToRemove = currentProject.getSubProjectByName(nameSubToRemove);
		currentProject.getSubProjectList().remove(subToRemove);
		support.firePropertyChange(PROP_SUB_REMOVED, null, subToRemove);
	}
	
	private static void switchToSubByName(String nameSubProject) {
		
		SubProject sub = currentProject.getSubProjectByName(nameSubProject);
		
		if (sub != null) {
			switchToSubProject(sub);
		} else if (currentProject.getSubProjectList().size() > 0){
			switchToSubProject(currentProject.getSubProjectList().get(0));
		} else {
			switchToSubProject(null);
		}
	}
	
	private static void loadOnlineMap(Project project, MapItem mapItem) {
		
		try {
			if (mapItem.getItem() != null) {
				
				WebMap webMap = WebMap.newInstance(mapItem.getItem());
				final SubProject onlineProject = new SubProject(project, "Online", false);
				onlineProject.setInitExtent(webMap.getInitExtent());
				
				if (webMap.getBaseMap() != null && webMap.getBaseMap().getBaseMapLayers() != null &&
						webMap.getBaseMap().getBaseMapLayers().size() > 0) {
					
					String tileServiceUrl = webMap.getBaseMap().getBaseMapLayers().get(0).getUrl();
					ArcGISTiledMapServiceLayer onlineTileLayer = new ArcGISTiledMapServiceLayer(tileServiceUrl);
					onlineTileLayer.setName("Online Basemap");
					onlineProject.setTiledLayer(onlineTileLayer);
					
					if (webMap.getOperationalLayers() != null) {
						for (WebMapLayer layer : webMap.getOperationalLayers()) {
							String featureServiceUrl = layer.getUrl();
							int lastIndexOf = featureServiceUrl.lastIndexOf("/");
							String url = featureServiceUrl.substring(0, lastIndexOf);
							String id = featureServiceUrl.substring(lastIndexOf + 1);
							final GeodatabaseFeatureServiceTable table = 
									new GeodatabaseFeatureServiceTable(url, PortalConnector.getUser(false), Integer.parseInt(id));
							table.initialize();
							onlineProject.addGeodatabaseFeatureTable(table);
							if (table.getGeodatabase() != null && table.getGeodatabase().isSyncEnabled()) {
								onlineProject.setSyncEnabled(true);
							}
							if (!table.isEditable())
								onlineProject.setEditable(false);
						}
					}
					project.addSubProject(onlineProject);
				}
			}
		} catch (Exception e) {
			logger.log(Level.SEVERE, "Fehler bei Initialisierung WebMap", e);
		}
	}
	
	private static void checkAndLoadOfflineProjects(Project project, MapItem mapItem) {
		List<SubProject> projectList = new ArrayList<>();
		try {
			Path offlineDataPath = Paths.get("data/offlineProjects/" + mapItem.getId());
			if (Files.exists(offlineDataPath) && Files.isDirectory(offlineDataPath)) {
				DirectoryStream<Path> offlineDirectoryStream = Files.newDirectoryStream(offlineDataPath);
				Iterator<Path> iterator = offlineDirectoryStream.iterator();
				while (iterator.hasNext()) {
					Path subPath = iterator.next();
					SubProject sub = loadOfflineSubProject(project, subPath);
					if (sub != null)
						projectList.add(sub);
				}
			}
		} catch (Throwable e) {
			logger.log(Level.SEVERE, "Subprojekt konnte nicht geladen werden", e);
		}
		
		project.addSubProjectList(projectList);
	}

	private static SubProject loadOfflineSubProject(Project project, Path subPath) throws IOException, FileNotFoundException {
		if (Files.isDirectory(subPath)) {
			SubProject offlineProject = new SubProject(project, subPath.getFileName().toString(), true);
			DirectoryStream<Path> projectDirectoryStream = Files.newDirectoryStream(subPath);
			Iterator<Path> projectIterator = projectDirectoryStream.iterator();
			while (projectIterator.hasNext()) {
				Path projectFile = projectIterator.next();
				if (projectFile.getFileName().toString().endsWith(".tpk")) {
					ArcGISLocalTiledLayer localTileLayer = new ArcGISLocalTiledLayer(projectFile.toString());
					localTileLayer.setName("Offline - " + subPath.getFileName().toString());
					offlineProject.setTiledLayer(localTileLayer);
				}
				if (projectFile.getFileName().toString().endsWith(".geodatabase")) {
					Geodatabase localGeodatabase = new Geodatabase(projectFile.toString());
					offlineProject.setFeatureData(localGeodatabase);
				}
			}
			
			return offlineProject;
		}
		return null;
	}

}
