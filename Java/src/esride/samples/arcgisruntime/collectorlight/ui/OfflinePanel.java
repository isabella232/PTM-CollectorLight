package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.io.File;
import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.imageio.ImageIO;
import javax.swing.JPanel;
import javax.swing.JScrollPane;

import org.codehaus.jackson.map.ObjectMapper;

import esride.samples.arcgisruntime.collectorlight.action.CreateOfflineProject;
import esride.samples.arcgisruntime.collectorlight.model.MapItem;
import esride.samples.arcgisruntime.collectorlight.model.MapItemList;

public class OfflinePanel extends JPanel {
	
	private static Logger logger = Logger.getLogger(OfflinePanel.class.getName());
	private BrowseMapsPanel browseMapsPanel;
	JScrollPane scrollPaneSearch;
	
	public OfflinePanel() {
		createUI();
		loadOfflineItems();
	}

	private void loadOfflineItems() {
		try {
			
			Path rootPath = Paths.get("data/offlineProjects");
			if (Files.exists(rootPath) && Files.isDirectory(rootPath)) {
				DirectoryStream<Path> offlineDirectoryStream = Files.newDirectoryStream(rootPath);
				Iterator<Path> iterator = offlineDirectoryStream.iterator();
				List<MapItem> myItems = new ArrayList<MapItem>();
				while (iterator.hasNext()) {
					Path projectPath = iterator.next();
					DirectoryStream<Path> projectDirectoryStream = Files.newDirectoryStream(projectPath);
					Iterator<Path> projectIterator = projectDirectoryStream.iterator();
					MapItem item = new MapItem(projectPath.getFileName().toString(), false);
					while (projectIterator.hasNext()) {
						Path projectFile = projectIterator.next();
						if (projectFile.getFileName().toString().equals("icon.png")) {
							try {
								File file = new File(projectFile.toString());
								item.setIcon(ImageIO.read(file));
							} catch (Exception e) {
								logger.log(Level.SEVERE, "Icon konnte nicht geöffnet werden", e);
							}
						} else if (projectFile.getFileName().toString().equals("portalitem.json")) {
							Files.readAllBytes(projectFile);
							ObjectMapper mapper = new ObjectMapper();
							Map jsonMap = mapper.readValue(Files.readAllBytes(projectFile), Map.class);
							if (jsonMap.containsKey("title")) {
								item.setTitle((String) jsonMap.get("title"));
							}
							if (jsonMap.containsKey("owner")) {
								item.setOwner((String) jsonMap.get("owner"));
							}
							if (jsonMap.containsKey("description")) {
								item.setSnippet((String) jsonMap.get("description"));
							}
						}
					}
					myItems.add(item);
				}
				MapItemList.getOfflineList().setItemList(myItems);
			} 
		} catch (IOException e) {
			logger.log(Level.SEVERE, "Fehler beim Laden der Offline Projekte", e);
		}
		
		
	}

	private void createUI() {
		setLayout(new BorderLayout());
		browseMapsPanel = new BrowseMapsPanel(false);
	    scrollPaneSearch = new JScrollPane(browseMapsPanel); 
	    scrollPaneSearch.setBorder(null);
	    add(scrollPaneSearch, BorderLayout.CENTER);
		
	}
}
