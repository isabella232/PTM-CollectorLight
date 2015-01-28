package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.Color;
import java.awt.Component;
import java.awt.Container;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.Insets;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;

import javax.swing.JPanel;
import javax.swing.SwingUtilities;

import esride.samples.arcgisruntime.collectorlight.model.MapItem;
import esride.samples.arcgisruntime.collectorlight.model.MapItemList;

public class BrowseMapsPanel extends JPanel implements PropertyChangeListener {
	
	boolean onlineMapItems;
	
	public BrowseMapsPanel(boolean onlineMapItems) {
		super();
		this.onlineMapItems = onlineMapItems;
		if (onlineMapItems) {
			MapItemList.getOnlineList().addListener(this);
		} else {
			MapItemList.getOfflineList().addListener(this);
		}
		
		setBackground(Color.WHITE); 
	    configureLayout();
	}

	private void configureLayout() {
		setLayout(new FlowLayout(FlowLayout.LEFT, 8, 8) { 
	      private static final long serialVersionUID = 1L; 
	      int bottomPadding = 20; 
	 
	      @Override 
	      public Dimension preferredLayoutSize(Container target) { 
	        synchronized (target.getTreeLock()) 
	        { 
	          int hgap = getHgap(); 
	          int vgap = getVgap(); 
	          int width = target.getWidth(); 
	 
	          Insets insets = target.getInsets(); 
	          if (insets == null) 
	            insets = new Insets(0, 0, 0, 0); 
	          int reqdWidth = 0; 
	 
	          int maxwidth = width - (insets.left + insets.right + hgap * 2); 
	          int n = target.getComponentCount(); 
	          int x = 0; 
	          int y = insets.top; 
	          int rowHeight = 0; 
	 
	          for (int i = 0; i < n; i++) { 
	            Component c = target.getComponent(i); 
	            if (c.isVisible()) { 
	              Dimension dim = c.getPreferredSize(); 
	              if ((x == 0) || ((x + dim.width) <= maxwidth)) { 
	                if (x > 0) { 
	                  x += hgap; 
	                } 
	                x += dim.width; 
	                rowHeight = Math.max(rowHeight, dim.height); 
	              } else { 
	                x = dim.width; 
	                y += vgap + rowHeight; 
	                rowHeight = dim.height; 
	              } 
	              reqdWidth = Math.max(reqdWidth, x); 
	            } 
	          } 
	          y += rowHeight; 
	          return new Dimension(reqdWidth+insets.left+insets.right, y + bottomPadding); 
	        } 
	      } 
	    });
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		if (evt.getPropertyName().equals(MapItemList.PROP_LIST_CHANGED)) {
			SwingUtilities.invokeLater(new Runnable() { 
			    @Override 
			    public void run() { 
			    	removeAll();
			    	for (MapItem item : onlineMapItems ? MapItemList.getOnlineList().getItemList() : MapItemList.getOfflineList().getItemList()) {
			    		PortalItemPanel portalItemPanel = new PortalItemPanel(item);
				        add(portalItemPanel);
			    	}
			    }
		    });
		}
		
	}

}
