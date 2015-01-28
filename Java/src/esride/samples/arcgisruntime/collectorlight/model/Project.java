package esride.samples.arcgisruntime.collectorlight.model;

import java.awt.image.BufferedImage;
import java.util.ArrayList;
import java.util.List;

public class Project {
	
	MapItem mapItem;
	List<SubProject> projetList = new ArrayList<SubProject>();

	public Project(MapItem mapItem) {
		this.mapItem = mapItem;
	}
	
	public String getId() {
		return mapItem.getId();
	}
	
	public String getTitle() {
		return mapItem.getTitle();
	}
	
	public String getDescription() {
		return mapItem.getSnippet();
	}
	
	public String getOwner() {
		return mapItem.getOwner();
	}
	
	public BufferedImage getIcon() {
		return mapItem.getIcon();
	}
	
	public MapItem getMapItem() {
		return mapItem;
	}

	public void addSubProject(SubProject sub) {
		projetList.add(sub);
	}
	
	public void addSubProjectList(List<SubProject> subList) {
		projetList.addAll(subList);
	}

	public List<SubProject> getSubProjectList() {
		return projetList;
	}
	
	public SubProject getSubProjectByName(String name) {
		if (name == null) {
			return null;
		}
		for (SubProject sub : projetList) {
			if (name.equals(sub.getName())) {
				return sub;
			}
		}
		return null;
	}
	
	public void dispose() {
		for (SubProject sub : getSubProjectList()) {
			sub.dispose();
		}
	}
}
