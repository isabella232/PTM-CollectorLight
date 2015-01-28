package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JComboBox;
import javax.swing.JComponent;
import javax.swing.JLabel;
import javax.swing.JLayeredPane;
import javax.swing.JList;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JProgressBar;
import javax.swing.JTabbedPane;
import javax.swing.JToolBar;
import javax.swing.ListCellRenderer;
import javax.swing.SwingUtilities;
import javax.swing.border.LineBorder;
import javax.swing.plaf.basic.BasicProgressBarUI;

import com.esri.map.FeatureLayer;
import com.esri.map.JMap;

import esride.samples.arcgisruntime.collectorlight.action.CreateOfflineProject;
import esride.samples.arcgisruntime.collectorlight.action.DeleteProject;
import esride.samples.arcgisruntime.collectorlight.action.SyncProject;
import esride.samples.arcgisruntime.collectorlight.model.Project;
import esride.samples.arcgisruntime.collectorlight.model.SubProject;
import esride.samples.arcgisruntime.collectorlight.services.MessageManager;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;

public class MapPanel extends JPanel implements PropertyChangeListener {

	private static final long serialVersionUID = 1L;
	
	public static final Font TOP_STATUS_FONT = new Font("Dialog", Font.ITALIC, 13);
	private static final String HOME_PANEL = "HOME_PANEL";
	static final String EDIT_PANEL = "EDIT_PANEL";
	static final String SYNC_PANEL = "SYNC_PANEL";
	private static Logger logger = Logger.getLogger(MapPanel.class.getName());
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	JLayeredPane mapPane;
	JToolBar toolBar;
	JLabel webMapStatusLabel;
	CreateOfflineProject createOfflineAction = new CreateOfflineProject();
	SyncProject syncAction = new SyncProject();
	DeleteProject deleteAction = new DeleteProject();
	JComboBox<SubProject> projectList;
	SelectExtentOverlay overlay;
	EditPanel editPanel;
	JProgressBar progressBar;
	
	public MapPanel() {
		ProjectManager.addListener(this);
		MessageManager.addListener(this);
		overlay = new SelectExtentOverlay();
	    overlay.setActive(false);
		createUI();
	}
	
	public void dispose() {
		if (mapPane.getComponentCountInLayer(JLayeredPane.DEFAULT_LAYER) == 1) {
			 JMap oldMap = (JMap) mapPane.getComponentsInLayer(JLayeredPane.DEFAULT_LAYER)[0];
			 oldMap.dispose();
		}
	}

	private void createUI() {
		
		setLayout(new BorderLayout(0, 1));
		addMainPanel();
	    addToolbar();
	}
	
	private void addToolbar() {
		
		toolBar = new JToolBar(); 
		toolBar.setFloatable(false);
	    add(toolBar, BorderLayout.NORTH);
	    
	    JButton homeButton = new SwitchPanelButton(HOME_PANEL, "res/home.png");
	    homeButton.setToolTipText(res.getString("MapPanel.HomeButton"));
	    toolBar.add(homeButton);
	    toolBar.addSeparator();
	    
	    JButton editButton = new SwitchPanelButton(EDIT_PANEL, "res/pen_grey.png");
	    editButton.setToolTipText(res.getString("MapPanel.EditButton"));
	    toolBar.add(editButton);
	    toolBar.addSeparator();
	    
	    JButton syncButton = new SwitchPanelButton(SYNC_PANEL, "res/sync.png");
	    syncButton.setToolTipText(res.getString("MapPanel.SyncButton"));
	    toolBar.add(syncButton);
	    toolBar.addSeparator(new Dimension(20, 45));
	    
	    JPanel homePanel = new JPanel();
	    createProjectSelection();
	    homePanel.add(projectList);
	    homePanel.setName(HOME_PANEL);
	    toolBar.add(HOME_PANEL, homePanel);
	    
	    editPanel = new EditPanel(this);
	    editPanel.setVisible(false);
	    toolBar.add(EDIT_PANEL, editPanel);
	    
	    JPanel syncPanel = new SyncPanel(this);
	    syncPanel.setVisible(false);
	    toolBar.add(SYNC_PANEL, syncPanel);
		
	}

	private void createProjectSelection() {
		projectList = new JComboBox<SubProject>();
		projectList.setMaximumSize(new Dimension(500, 25)); 
		projectList.setAlignmentX(JComponent.LEFT_ALIGNMENT);
		projectList.setRenderer(new ProjectComboBoxRenderer());
		projectList.addActionListener(new ActionListener() {
			@Override
			public void actionPerformed(ActionEvent e) {
				ProjectManager.switchToSubProject((SubProject)projectList.getSelectedItem());
			}
		});
	}
	
	private void addMainPanel() {
		mapPane = createLayeredPane();
		progressBar = createProgressBar(mapPane); 
		mapPane.add(progressBar);
		mapPane.setLayer(progressBar, JLayeredPane.PALETTE_LAYER);
	 
	    add(mapPane, BorderLayout.CENTER); 
	}
	
	private static JLayeredPane createLayeredPane() { 
		 JLayeredPane contentPane = new JLayeredPane(); 
		 contentPane.setSize(1000, 700); 
		 contentPane.setLayout(new BorderLayout()); 
		 contentPane.setVisible(true); 
		 return contentPane; 
	}
	
	private static JProgressBar createProgressBar(final JComponent parent) { 
		 final JProgressBar localProgressBar = new JProgressBar(); 
		 localProgressBar.setSize(400, 25); 
		 localProgressBar.setBackground(Color.DARK_GRAY); 
		 localProgressBar.setBorderPainted(false); 
		 localProgressBar.setForeground(Color.WHITE); 
		 localProgressBar.setUI(new BasicProgressBarUI() { 
		      @Override 
		      protected Color getSelectionBackground() { return Color.WHITE; } 
		      @Override 
		      protected Color getSelectionForeground() { return Color.DARK_GRAY; } 
		    });
		 parent.addComponentListener(new ComponentAdapter() { 
		   @Override 
		   public void componentResized(ComponentEvent e) { 
			   localProgressBar.setLocation( 
		         parent.getWidth()/2 - localProgressBar.getWidth()/2, 
		         parent.getHeight() - localProgressBar.getHeight() - 20); 
		   } 
		 }); 
		 localProgressBar.setStringPainted(true); 
		 localProgressBar.setIndeterminate(true);
		 localProgressBar.setVisible(false); 
		 return localProgressBar; 
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		if (evt.getPropertyName().equals(ProjectManager.PROP_PROJECT_CHANGED)) {
			final Project project = (Project) evt.getNewValue();
			if (project != null) {
				SwingUtilities.invokeLater(new Runnable() {
					
					@Override
					public void run() {
						openProject(project);
					}
				});
			}
		} else if (evt.getPropertyName().equals(ProjectManager.PROP_SUB_CHANGED)) {
			final SubProject sub = (SubProject)evt.getNewValue();
			if (sub == null) {
				return;
			}
			SwingUtilities.invokeLater(new Runnable() {
				
				@Override
				public void run() {
					openMap(sub);
				}
			});
		} else if (evt.getPropertyName().equals(ProjectManager.PROP_SUB_ADDED)) {
			final SubProject sub = (SubProject)evt.getNewValue();
			if (sub == null) {
				return;
			}
			SwingUtilities.invokeLater(new Runnable() {
				
				@Override
				public void run() {
					projectList.addItem(sub);
					projectList.setSelectedItem(sub);
				}
			});
		} else if (evt.getPropertyName().equals(ProjectManager.PROP_SUB_REMOVED)) {
			final SubProject sub = (SubProject)evt.getNewValue();
			if (sub == null) {
				return;
			}
			SwingUtilities.invokeLater(new Runnable() {
				
				@Override
				public void run() {
					projectList.removeItem(sub);
				}
			});
		} else if (evt.getPropertyName().equals(MessageManager.PROGRESS_MESSAGE)) {
			progressBar.setString((String) evt.getNewValue());
		} else if (evt.getPropertyName().equals(MessageManager.PROGRESS_VISIBLE)) {
			progressBar.setVisible((boolean) evt.getNewValue());
		}
		
	}
	
	private void openProject(Project mapProject) {
		try { 
			projectList.removeAllItems();
		    if (mapProject.getSubProjectList().size() > 0) {
		    	for (SubProject proj : mapProject.getSubProjectList()) {
					projectList.addItem(proj);
				}
		    }
	    } catch (Exception e) { 
	    	JOptionPane.showMessageDialog(null, res.getString("MapPanel.Error") + e.getLocalizedMessage(),
	    			"Warning", JOptionPane.WARNING_MESSAGE);
	    	logger.log(Level.SEVERE, "Projekt konnte nicht geöffnet werden", e);
	    }
	    JTabbedPane parent = (JTabbedPane) this.getParent();
		parent.setSelectedComponent(this);
	}
	
	private void openMap(final SubProject project) {
    	closeExistingMap(true); 
	    JMap newMap = new JMap();
	    newMap.getLayers().add(project.getTiledLayer());
    	for (FeatureLayer featureLayer : project.getFeatureLayer()) {
    		newMap.getLayers().add(featureLayer);
    	}
//		    	if (project.getInitExtent() != null) {
//		    		newMap.zoomTo(project.getInitExtent());
//		    	}
    	newMap.addMapOverlay(overlay);
    	mapPane.add(newMap, 0);
    	mapPane.setLayer(newMap, JLayeredPane.DEFAULT_LAYER);
    	if (project.isEditable())
    		editPanel.setMap(newMap);
    	createOfflineAction.setMap(newMap);
	}
	 
	 void closeExistingMap(boolean removeLayers) {
		 if (mapPane.getComponentCountInLayer(JLayeredPane.DEFAULT_LAYER) == 1) {
			 JMap oldMap = (JMap) mapPane.getComponentsInLayer(JLayeredPane.DEFAULT_LAYER)[0];
			 oldMap.removeMapOverlay(overlay);
			 mapPane.remove(oldMap);
			 editPanel.setMap(null);
			 if (removeLayers) {
				 oldMap.getLayers().removeAll(oldMap.getLayers());
			 }
			 if (oldMap.isReady()) {
				 oldMap.dispose();
			 }
		 }
	 }
	 
	 private class ProjectComboBoxRenderer extends JLabel implements ListCellRenderer<SubProject> {

			private static final long serialVersionUID = 1L;

			@Override
			public Component getListCellRendererComponent(
					JList<? extends SubProject> list, SubProject value, int index,
					boolean isSelected, boolean cellHasFocus) {
				if (isSelected) { 
				    setBorder(new LineBorder(list.getSelectionBackground(), 2)); 
				} else { 
				    setBackground(list.getBackground()); 
				    setForeground(list.getForeground()); 
				    setBorder(new LineBorder(list.getBackground(), 2)); 
				}
				if (value != null) {
					setText(value.getName());
				}
				return this;
			}
		}
	 
	private class SwitchPanelButton extends JButton {
		private static final long serialVersionUID = 1L;

		public SwitchPanelButton(final String panelIdentifier, String iconUrl) {
			
			super(new ImageIcon(AdministrationPanel.class.getResource(iconUrl)));
			addActionListener(new ActionListener() {
				
				@Override
				public void actionPerformed(ActionEvent e) {
					for (Component component : toolBar.getComponents()) {
						if (component.getName() != null) {
							if (component.getName().equals(panelIdentifier)) {
								component.setVisible(true);
							} else {
								component.setVisible(false);
							}
						}
					}
				}
			});
		}
	}

}
