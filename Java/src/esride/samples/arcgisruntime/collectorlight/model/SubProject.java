package esride.samples.arcgisruntime.collectorlight.model;

import java.util.ArrayList;
import java.util.List;

import com.esri.core.geodatabase.Geodatabase;
import com.esri.core.geodatabase.GeodatabaseFeatureTable;
import com.esri.core.geometry.Envelope;
import com.esri.map.FeatureLayer;
import com.esri.map.Layer;

public class SubProject {
	
	Layer tiledLayer;
	Geodatabase featureData;
	List<GeodatabaseFeatureTable> featureTableList;
	List<FeatureLayer> featureLayerList = new ArrayList<FeatureLayer>();;
	boolean isOffline;
	boolean isSyncEnabled;
	boolean isEditable;
	String name;
	Envelope initExtent;
	Project parentProject;
	
	public SubProject(Project parentProject, String name, boolean isOffline) {
		this.parentProject = parentProject;
		this.name = name;
		this.isOffline = isOffline;
		isSyncEnabled = false;
		isEditable = true;
	}
	
	public boolean isOffline() {
		return isOffline;
	}
	
	public Layer getTiledLayer() {
		return tiledLayer;
	}
	public void setTiledLayer(Layer tiledLayer) {
		this.tiledLayer = tiledLayer;
	}
	public Geodatabase getFeatureData() {
		return featureData;
	}
	public void setFeatureData(Geodatabase featureData) {
		this.featureData = featureData;
		this.featureTableList = featureData.getGeodatabaseTables();
		for (GeodatabaseFeatureTable table : this.featureTableList) {
			if (!table.isEditable())
				setEditable(false);
			FeatureLayer featureLayer = new FeatureLayer(table);
			featureLayer.initializeAsync();
			featureLayerList.add(featureLayer);
		}
	}
	
	public String getName() {
		return name;
	}

	public List<GeodatabaseFeatureTable> getFeatureTableList() {
		return featureTableList;
	}

	public void addGeodatabaseFeatureTable(GeodatabaseFeatureTable featureTable) {
		if (!isOffline) {
			if (featureTableList == null) {
				featureTableList = new ArrayList<GeodatabaseFeatureTable>();
			}
			featureTableList.add(featureTable);
			FeatureLayer featureLayer = new FeatureLayer(featureTable);
			featureLayer.initializeAsync();
			featureLayerList.add(featureLayer);
		}
	}

	public List<FeatureLayer> getFeatureLayer() {
		return featureLayerList;
	}

	public Envelope getInitExtent() {
		return initExtent;
	}

	public void setInitExtent(Envelope initExtent) {
		this.initExtent = initExtent;
	}

	public Project getParentProject() {
		return parentProject;
	}

	public boolean isSyncEnabled() {
		return isSyncEnabled;
	}

	public void setSyncEnabled(boolean isSyncEnabled) {
		this.isSyncEnabled = isSyncEnabled;
	}

	public boolean isEditable() {
		return isEditable;
	}

	public void setEditable(boolean isEditable) {
		this.isEditable = isEditable;
	}
	
	public void dispose() {
		if (isOffline() && getFeatureData() != null) {
			getFeatureData().dispose();
		}
	}
}
