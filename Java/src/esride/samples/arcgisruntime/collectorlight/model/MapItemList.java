package esride.samples.arcgisruntime.collectorlight.model;

import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;
import java.util.ArrayList;
import java.util.List;

public class MapItemList {
	
	public static final String PROP_LIST_CHANGED = "MapItemListChanged";	
	
	private static MapItemList onlineList = new MapItemList();
	private static MapItemList offlineList = new MapItemList();
	
	private final PropertyChangeSupport support;	
	List<MapItem> itemList = new ArrayList<MapItem>();
	
	private MapItemList() {
		support = new PropertyChangeSupport(this);
	}
	
	public static MapItemList getOnlineList() {
		return onlineList;
	}
	
	public static MapItemList getOfflineList() {
		return offlineList;
	}
	
	public void addListener(PropertyChangeListener listener) {
		support.addPropertyChangeListener(listener);
	}
	
	public MapItem getItemById(String id) {
		for (MapItem item : itemList) {
			if (item.getId().equals(id)) {
				return item;
			}
		}
		return null;
	}
	
	public void addItem(MapItem item) {
		List<MapItem> oldValue = itemList;
		itemList.add(item);
		support.firePropertyChange(PROP_LIST_CHANGED, oldValue, itemList);
	}
	
	public void removeItem(MapItem item) {
		itemList.remove(item);
	}
	
	public void setItemList(List<MapItem> itemList) {
		support.firePropertyChange(PROP_LIST_CHANGED, this.itemList, itemList);
		this.itemList = itemList;
	}
	
	public List<MapItem> getItemList() {
		return itemList;
	}

}
