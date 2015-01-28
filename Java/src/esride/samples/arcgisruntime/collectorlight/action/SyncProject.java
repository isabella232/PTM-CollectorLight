package esride.samples.arcgisruntime.collectorlight.action;

import java.awt.event.ActionEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.Map;
import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.swing.AbstractAction;

import com.esri.core.geodatabase.Geodatabase;
import com.esri.core.geodatabase.GeodatabaseEditError;
import com.esri.core.geodatabase.GeodatabaseFeatureTable;
import com.esri.core.geodatabase.GeodatabaseFeatureTableEditErrors;
import com.esri.core.map.CallbackListener;
import com.esri.core.tasks.geodatabase.GeodatabaseStatusCallback;
import com.esri.core.tasks.geodatabase.GeodatabaseStatusInfo;
import com.esri.core.tasks.geodatabase.GeodatabaseSyncTask;
import com.esri.core.tasks.geodatabase.SyncGeodatabaseParameters;

import esride.samples.arcgisruntime.collectorlight.model.SubProject;
import esride.samples.arcgisruntime.collectorlight.services.MessageManager;
import esride.samples.arcgisruntime.collectorlight.services.PortalConnector;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;

public class SyncProject extends AbstractAction implements PropertyChangeListener {
	
	private static Logger logger = Logger.getLogger(SyncProject.class.getName());
	private static final long serialVersionUID = 1L;
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	Geodatabase gdb = null;
	String featureServiceUrl;
	GeodatabaseSyncTask gdbTask;
	private SubProject currentProject;
	
	public SyncProject() {
		super();
		ProjectManager.addListener(this);
	}
	
	@Override
	public boolean isEnabled() {
		if (currentProject == null || !currentProject.isOffline() || gdb == null || !gdb.isSyncEnabled())
			return false;
		return true;
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		
		Runnable runnable = new Runnable() {
			
			@Override
			public void run() {
			  
				try {
					// main parameters
					final SyncGeodatabaseParameters syncParams = gdb.getSyncParameters();
		
					CallbackListener<Map<Integer, GeodatabaseFeatureTableEditErrors>> syncResponseCallback = 
							new CallbackListener<Map<Integer, GeodatabaseFeatureTableEditErrors>>() {
						@Override 
					    public void onError(Throwable e) {
							logger.log(Level.SEVERE, "Error in sync response callback: ", e);
							MessageManager.addTextMessage(res.getString("SyncAction.Error") + e.getMessage());
							MessageManager.hideProgress();
					    } 
		
						@Override
						public void onCallback(
								Map<Integer, GeodatabaseFeatureTableEditErrors> objs) {
							MessageManager.hideProgress();
							if (objs != null) {
								for (Integer key : objs.keySet()) {
									GeodatabaseFeatureTableEditErrors errors = objs.get(key);
									for (GeodatabaseEditError error : errors.getFeatureEditResults()) {
										logger.log(Level.SEVERE, "Feature Error for Key: " + error.getError());
										MessageManager.addTextMessage(res.getString("SyncAction.FeatureError") + error.getError());
									}
									for (GeodatabaseEditError error : errors.getAttachmentEditResults()) {
										logger.log(Level.SEVERE, "Attachement Error for Key: " + error.getError());
										MessageManager.addTextMessage(res.getString("SyncAction.AttachementError") + error.getError());
									}
								}
							}
						}
					}; 
		
					GeodatabaseStatusCallback statusCallback = new GeodatabaseStatusCallback() {
						@Override 
					    public void statusUpdated(GeodatabaseStatusInfo status) {
							MessageManager.addTextMessage(res.getString("SyncAction.Status") + status.getStatus());
							MessageManager.setProgressMessage(res.getString("SyncAction.Status") + status.getStatus());
							if (status.getError() != null) {
								logger.log(Level.SEVERE, "Sync Fehler Code: " + status.getError().getCode());
								MessageManager.addTextMessage(res.getString("SyncAction.ErrorCode") + status.getError().getCode());
								logger.log(Level.SEVERE, "Sync Fehler Beschreibung: " + status.getError().getDescription());
								MessageManager.addTextMessage(res.getString("SyncAction.ErrorDescription") + status.getError().getDescription());
								logger.log(Level.SEVERE, "Sync Fehler Message: " + status.getError().getThrowable().getMessage());
								MessageManager.addTextMessage(res.getString("SyncAction.ErrorMessage") + status.getError().getThrowable().getMessage());
							}
					    }
					};
		
					// ------------------------------------------------------------------------ 
					// Start sync 
					// ------------------------------------------------------------------------ 
					if (gdbTask == null) {
						gdbTask = new GeodatabaseSyncTask(featureServiceUrl, PortalConnector.getUser(true)); 
					}
					gdbTask.syncGeodatabase(syncParams, gdb, statusCallback, syncResponseCallback);
					MessageManager.setProgressMessage(res.getString("SyncAction.Start"));
					MessageManager.showProgress();
				} catch (Exception e1) {
					logger.log(Level.SEVERE, "Error in SyncGeodatabase", e1);
					MessageManager.addTextMessage(res.getString("SyncAction.Error") + e1.getMessage());
					MessageManager.hideProgress();
				}
			}
		};
		Thread t = new Thread(runnable);
		t.start();
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
