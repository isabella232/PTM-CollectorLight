package esride.samples.arcgisruntime.collectorlight.action;

import java.awt.event.ActionEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.io.IOException;
import java.nio.file.DirectoryStream;
import java.nio.file.FileVisitResult;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.SimpleFileVisitor;
import java.nio.file.attribute.BasicFileAttributes;
import java.util.Iterator;
import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.swing.AbstractAction;

import com.esri.core.geodatabase.Geodatabase;
import com.esri.core.geodatabase.GeodatabaseFeatureTable;
import com.esri.core.map.CallbackListener;
import com.esri.core.tasks.geodatabase.GeodatabaseSyncTask;
import com.esri.core.tasks.geodatabase.GeodatabaseTaskResult;

import esride.samples.arcgisruntime.collectorlight.model.SubProject;
import esride.samples.arcgisruntime.collectorlight.services.MessageManager;
import esride.samples.arcgisruntime.collectorlight.services.PortalConnector;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;

public class DeleteProject extends AbstractAction implements PropertyChangeListener {
	
	private static final long serialVersionUID = 1L;
	private static Logger logger = Logger.getLogger(DeleteProject.class.getName());
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	Geodatabase gdb = null;
	GeodatabaseSyncTask gdbTask;
	String featureServiceUrl;
	private SubProject currentProject;
	
	public DeleteProject() {
		super("Delete project");
		ProjectManager.addListener(this);
		setEnabled(isEnabled());
	}
	
	@Override
	public boolean isEnabled() {
		if (currentProject == null || !currentProject.isOffline())
			return false;
		return true;
	}
	
	@Override
	public void actionPerformed(ActionEvent e) {
		Runnable runnable = new Runnable() {
			
			@Override
			public void run() {
				if (gdb != null) {
					unregisterGeodatabase();
				} else {
					deleteFiles();
				}
			}
		};
		Thread t = new Thread(runnable);
		t.start();
	}
	
	private void deleteFiles() {
		gdb.dispose();
		String url = ProjectManager.getPathForOfflineSubProject(currentProject.getParentProject().getId(), currentProject.getName());
		Path path = Paths.get(url);
		if (Files.exists(path)) {
			try {
				Files.walkFileTree(path, new SimpleFileVisitor<Path>() {
					   @Override
					   public FileVisitResult visitFile(Path file, BasicFileAttributes attrs) throws IOException {
						   Files.delete(file);
						   return FileVisitResult.CONTINUE;
					   }

					   @Override
					   public FileVisitResult postVisitDirectory(Path dir, IOException exc) throws IOException {
						   Files.delete(dir);
						   return FileVisitResult.CONTINUE;
					   }

				   });
				Path parentPath = path.getParent();
				Files.delete(path);
				DirectoryStream<Path> offlineDirectoryStream = Files.newDirectoryStream(parentPath);
				Iterator<Path> iterator = offlineDirectoryStream.iterator();
				boolean hasAnotherSubProject = false;
				while (iterator.hasNext()) {
					Path subPath = iterator.next();
					if (Files.isDirectory(subPath)) {
						hasAnotherSubProject = true;
					}
				}
				if (!hasAnotherSubProject) {
					Files.walkFileTree(parentPath, new SimpleFileVisitor<Path>() {
						   @Override
						   public FileVisitResult visitFile(Path file, BasicFileAttributes attrs) throws IOException {
							   Files.delete(file);
							   return FileVisitResult.CONTINUE;
						   }

						   @Override
						   public FileVisitResult postVisitDirectory(Path dir, IOException exc) throws IOException {
							   Files.delete(dir);
							   return FileVisitResult.CONTINUE;
						   }

					   });
				}
				Files.delete(parentPath);
			} catch (IOException e) {
				logger.log(Level.SEVERE, "Error in delete files", e);
				MessageManager.addTextMessage(res.getString("DeleteAction.Error") + e.getMessage());
			} finally {
				ProjectManager.removeSubFromCurrentProject(currentProject.getParentProject(), currentProject.getName());
			}
		}
	}

	private void unregisterGeodatabase() {
		try {
			
			CallbackListener<GeodatabaseTaskResult> unregisterCallback = new CallbackListener<GeodatabaseTaskResult>() {
	
				@Override
				public void onCallback(GeodatabaseTaskResult objs) {
					logger.log(Level.INFO, "On Callback " + objs.succeeded());
					logger.log(Level.INFO, "Messages " + objs.getMessage());
					logger.log(Level.INFO, objs.getDetails());
					logger.log(Level.INFO, objs.getCode());
					MessageManager.addTextMessage(res.getString("DeleteAction.UnregisterCallback") + objs.succeeded());
					MessageManager.addTextMessage(res.getString("DeleteAction.UnregisterMessage") + objs.getMessage());
					MessageManager.addTextMessage(res.getString("DeleteAction.UnregisterDetail") + objs.getDetails());
					MessageManager.addTextMessage(res.getString("DeleteAction.UnregisterCode") + objs.getCode());
					MessageManager.setProgressMessage(res.getString("DeleteAction.DeleteFiles"));
					deleteFiles();
					MessageManager.hideProgress();
				}
	
				@Override
				public void onError(Throwable e) {
					logger.log(Level.SEVERE, "Error in unregister callback: ", e);
					MessageManager.addTextMessage(res.getString("DeleteAction.ErrorUnregister") + e.getMessage());
					MessageManager.setProgressMessage(res.getString("DeleteAction.DeleteFiles"));
					deleteFiles();
					MessageManager.hideProgress();
				}
			};
			
			// ------------------------------------------------------------------------ 
			// Start unregister
			// ------------------------------------------------------------------------ 
			if (gdbTask == null) {
				gdbTask = new GeodatabaseSyncTask(featureServiceUrl, PortalConnector.getUser(true)); 
			}
			gdbTask.unregisterGeodatabase(gdb, unregisterCallback);
			MessageManager.setProgressMessage(res.getString("DeleteAction.StartUnregister"));
			MessageManager.showProgress();
		} catch (Exception e1) {
			logger.log(Level.SEVERE, "Fehler in Unregister GDB", e1);
			MessageManager.addTextMessage(res.getString("DeleteAction.ErrorUnregister") + e1.getMessage());
			MessageManager.hideProgress();
		}
	}
	
	private void setSubProject(SubProject subProject) {
		currentProject = subProject;
		
		if (currentProject != null && currentProject.getFeatureTableList() != null) {
			GeodatabaseFeatureTable table = currentProject.getFeatureTableList().get(0);
			gdb = table.getGeodatabase();
			if (gdb != null) {
				featureServiceUrl = gdb.getServiceURL();
			}
		}
		
		setEnabled(isEnabled());
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		if (evt.getPropertyName().equals(ProjectManager.PROP_SUB_CHANGED)) {
			setSubProject((SubProject) evt.getNewValue());
		}
	}

}
