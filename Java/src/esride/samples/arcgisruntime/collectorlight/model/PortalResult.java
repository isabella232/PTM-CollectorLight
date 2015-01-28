package esride.samples.arcgisruntime.collectorlight.model;

import java.util.List;

public class PortalResult {
	
	 private List<MapItem> myItems; 
	 private int numResults; 
	     
	 public int getNumResults() { 
		 return numResults; 
	 }
	 
	 public void setNumResults(int numResults) { 
		 this.numResults = numResults; 
	 }
	 
	 public List<MapItem> getItems() { 
		 return myItems; 
	 }
	 
	 public void setItems(List<MapItem> list) { 
		 this.myItems = list; 
	 }
}
