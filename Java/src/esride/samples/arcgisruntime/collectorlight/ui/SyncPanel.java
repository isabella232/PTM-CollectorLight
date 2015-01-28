package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.ResourceBundle;

import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JPanel;
import javax.swing.JSeparator;
import javax.swing.JTextField;
import javax.swing.JToggleButton;
import javax.swing.event.DocumentEvent;
import javax.swing.event.DocumentListener;

public class SyncPanel extends JPanel {
	
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	MapPanel mapPanel;
	
	public SyncPanel(MapPanel mapPanel) {
		super();
		this.mapPanel = mapPanel;
		this.setName(MapPanel.SYNC_PANEL);
		createComponents();
	}

	private void createComponents() {
		
	    JButton createOfflineButton = new JButton(mapPanel.createOfflineAction);
	    createOfflineButton.setIcon(new ImageIcon(MapPanel.class.getResource("res/download.jpg")));
	    createOfflineButton.setToolTipText(res.getString("SyncPanel.DownloadButton"));
	    this.add(createOfflineButton);
	    JTextField nameInput = new JTextField(20);
	    nameInput.getDocument().addDocumentListener(createProjectNameListener(nameInput));
	    
	    nameInput.setText(res.getString("SyncPanel.NameInputDefault"));
	    nameInput.setToolTipText(res.getString("SyncPanel.NameInputTooltip"));
	    JTextField minLevelInput = new JTextField(3);
	    JTextField maxLevelInput = new JTextField(3);
	    DocumentListener listener = createLevelListener(minLevelInput, maxLevelInput);
	    minLevelInput.getDocument().addDocumentListener(listener);
	    maxLevelInput.getDocument().addDocumentListener(listener);
	    minLevelInput.setText("3");
	    maxLevelInput.setText("17");
	    JToggleButton extentButton = new JToggleButton(res.getString("SyncPanel.ExtentButtonText"));
	    extentButton.setToolTipText(res.getString("SyncPanel.ExtentButtonTooltip"));
	    extentButton.addActionListener(new ActionListener() {

	        @Override
	        public void actionPerformed(ActionEvent e) {
	          if (((JToggleButton)e.getSource()).isSelected()) {
	            mapPanel.overlay.setActive(true);
	          } else {
	        	  mapPanel.overlay.setActive(false);
	          }
	        }
	    });
	    this.add(nameInput);
	    this.add(minLevelInput);
	    this.add(maxLevelInput);
	    this.add(extentButton);
	    this.add(new JSeparator());
	    JButton syncGeodatabaseButton = new JButton(mapPanel.syncAction);
	    syncGeodatabaseButton.setIcon(new ImageIcon(MapPanel.class.getResource("res/upload.png")));
	    syncGeodatabaseButton.setToolTipText(res.getString("SyncPanel.SyncButton"));
	    this.add(syncGeodatabaseButton);
	    JButton deleteOfflineButton = new JButton(new ImageIcon(MapPanel.class.getResource("res/delete.png")));
	    deleteOfflineButton.setEnabled(mapPanel.deleteAction.isEnabled());
	    deleteOfflineButton.setToolTipText(res.getString("SyncPanel.DeleteButton"));
	    mapPanel.deleteAction.addPropertyChangeListener(new PropertyChangeListener() {
			
			@Override
			public void propertyChange(PropertyChangeEvent evt) {
				deleteOfflineButton.setEnabled(mapPanel.deleteAction.isEnabled());
				
			}
		});
	    deleteOfflineButton.addActionListener(new ActionListener() {
			
			@Override
			public void actionPerformed(ActionEvent e) {
				mapPanel.closeExistingMap(false);
				mapPanel.deleteAction.actionPerformed(null);
				
			}
		});
	    this.add(deleteOfflineButton);
		
	}
	
	private DocumentListener createLevelListener(JTextField minLevelInput, JTextField maxLevelInput) {
		DocumentListener levelListener = new DocumentListener() {
				
				@Override
				public void removeUpdate(DocumentEvent e) {
					try {
						mapPanel.createOfflineAction.setLevel(Integer.parseInt(minLevelInput.getText()), Integer.parseInt(maxLevelInput.getText()));
					} catch (NumberFormatException e1) {
					}
				}
				
				@Override
				public void insertUpdate(DocumentEvent e) {
					try {
						mapPanel.createOfflineAction.setLevel(Integer.parseInt(minLevelInput.getText()), Integer.parseInt(maxLevelInput.getText()));
					} catch (NumberFormatException e1) {
					}
				}
				
				@Override
				public void changedUpdate(DocumentEvent e) {
					try {
						mapPanel.createOfflineAction.setLevel(Integer.parseInt(minLevelInput.getText()), Integer.parseInt(maxLevelInput.getText()));
					} catch (NumberFormatException e1) {
					}
				}
			};
		return levelListener;
	 }
	
	private DocumentListener createProjectNameListener(JTextField nameInput) {
		return new DocumentListener() {
			
			@Override
			public void removeUpdate(DocumentEvent e) {
				mapPanel.createOfflineAction.setProjectName(nameInput.getText());
			}
			
			@Override
			public void insertUpdate(DocumentEvent e) {
				mapPanel.createOfflineAction.setProjectName(nameInput.getText());
			}
			
			@Override
			public void changedUpdate(DocumentEvent e) {
				mapPanel.createOfflineAction.setProjectName(nameInput.getText());
			}
		};
	}

}
