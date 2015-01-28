package esride.samples.arcgisruntime.collectorlight.model;

import java.awt.image.BufferedImage;
import java.io.ByteArrayInputStream;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.imageio.ImageIO;
import javax.swing.Icon;
import javax.swing.ImageIcon;

import com.esri.core.portal.PortalItem;

public class MapItem {
	
	private static Logger logger = Logger.getLogger(MapItem.class.getName());
	
	String id;
	String title;
	String snippet;
	BufferedImage icon;
	String owner;
	PortalItem item;
	boolean onlineItem;
	
	public MapItem(PortalItem item, boolean onlineItem) {
		setItem(item);
		this.onlineItem = onlineItem;
	}
	
	public MapItem(String id, boolean onlineItem) {
		this.id = id;
		this.onlineItem = onlineItem;
	}
	
	public boolean isOnlineItem() {
		return onlineItem;
	}
	
	public String getId() {
		return id;
	}
	
	public String getTitle() {
		return title;
	}
	public void setTitle(String title) {
		this.title = title;
	}
	public String getSnippet() {
		return snippet;
	}
	public void setSnippet(String snippet) {
		this.snippet = snippet;
	}
	public BufferedImage getIcon() {
		return icon;
	}
	public void setIcon(BufferedImage icon) {
		this.icon = icon;
	}
	
	public String getOwner() {
		return owner;
	}
	public void setOwner(String owner) {
		this.owner = owner;
	}
	public PortalItem getItem() {
		return item;
	}
	public void setItem(PortalItem item) {
		this.item = item;
		this.id = item.getItemId();
		try {
			ByteArrayInputStream inStram = new ByteArrayInputStream(item.fetchThumbnail());
			icon = ImageIO.read(inStram);
	    } catch (Exception e) {
	    	logger.log(Level.SEVERE, "Fehler bei thumbnail fetch " + item.getThumbnailFileName(), e);
	    }
		this.title = item.getTitle();
		this.snippet = item.getSnippet();
		this.owner = item.getOwner();
	}

}
