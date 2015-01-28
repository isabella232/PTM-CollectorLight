package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.event.ComponentEvent;
import java.awt.event.ComponentListener;

import javax.swing.JList;
import javax.swing.JPanel;

import com.esri.map.JMap;
import com.esri.toolkit.editing.JEditToolsPicker;
import com.esri.toolkit.editing.JTemplatePicker;

public class EditPanel extends JPanel {
	
	JMap map;
	MapPanel mapPanel;
	JTemplatePicker jPicker;
	JEditToolsPicker editToolsPicker;
	
	public EditPanel(MapPanel mapPanel) {
		super();
		this.mapPanel = mapPanel;
		this.addComponentListener(new ComponentListener() {
			
			@Override
			public void componentShown(ComponentEvent e) {
				if (jPicker != null)
					jPicker.setVisible(true);
				
			}
			@Override
			public void componentHidden(ComponentEvent e) {
				if (jPicker != null)
					jPicker.setVisible(false);
				
			}
			@Override
			public void componentResized(ComponentEvent e) {
			}
			@Override
			public void componentMoved(ComponentEvent e) {
			}
		});
		this.setName(MapPanel.EDIT_PANEL);
	}
	
	void setMap(JMap map) {
		this.map = map;
		createComponents();
	}
	
	private void createComponents() {
		this.removeAll();
		if (jPicker != null)
			mapPanel.remove(jPicker);
		
		 if (map != null) {
			 
			 // add template picker
			 jPicker = new JTemplatePicker(map);
			 jPicker.setIconWidth(18); 
		     jPicker.setIconHeight(18);
		     jPicker.setLayoutOrientation(JList.VERTICAL);
		     jPicker.setShowNames(true); 
		     jPicker.setWatchMap(true);
		     jPicker.addLayersFromMap();
		     jPicker.setVisible(false);
		     jPicker.setEnabled(true);
			 jPicker.setShowAttributeEditor(true);
			 mapPanel.add(jPicker, BorderLayout.WEST);
			 
			// add edit tools picker
			editToolsPicker = new JEditToolsPicker(map);
			editToolsPicker.setCreationOverlay(jPicker.getOverlay());
			this.add(editToolsPicker);
		}
	}

}
