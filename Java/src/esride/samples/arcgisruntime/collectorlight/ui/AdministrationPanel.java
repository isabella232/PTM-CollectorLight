package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.CardLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.ResourceBundle;

import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JPanel;
import javax.swing.JToolBar;

import esride.samples.arcgisruntime.collectorlight.services.PortalConnector;

public class AdministrationPanel extends JPanel implements PropertyChangeListener {
	
	private static final String ONLINE_PANEL = "ONLINE_PANEL";
	private static final String OFFLINE_PANEL = "OFFLINE_PANEL";
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	JPanel mainPanel;
	JButton loginButton;
	JButton logoutButton;
	
	public AdministrationPanel() {
		createUI();
		PortalConnector.addListener(this);
	}

	private void createUI() {
		setLayout(new BorderLayout());
		addToolbar();
		addMainPanel();
	}

	private void addMainPanel() {
			    
	    JPanel offlinePanel = new OfflinePanel();
	    
	    OnlinePanel onlinePanel = new OnlinePanel();
	    
	    mainPanel = new JPanel();
		CardLayout layout = new CardLayout(0, 0);
	    mainPanel.setLayout(layout);
	    mainPanel.add(onlinePanel, ONLINE_PANEL);
	    mainPanel.add(offlinePanel, OFFLINE_PANEL);
	    add(mainPanel, BorderLayout.CENTER);
		
	}

	private void addToolbar() {
		
		JToolBar toolBar = new JToolBar(); 
		toolBar.setFloatable(false);
	    add(toolBar, BorderLayout.NORTH);
	    
	    JButton onlineButton = new SwitchPanelButton(ONLINE_PANEL, "res/map_online.png");
	    onlineButton.setToolTipText(res.getString("AdminPanel.OnlineButton"));
	    toolBar.add(onlineButton);
	    toolBar.addSeparator();
	    
	    JButton offlineButton = new SwitchPanelButton(OFFLINE_PANEL, "res/map_offline.png");
	    offlineButton.setToolTipText(res.getString("AdminPanel.OfflineButton"));
	    toolBar.add(offlineButton);
	    toolBar.addSeparator();
	    
	    loginButton = new JButton();
	    loginButton.setIcon(new ImageIcon(AdministrationPanel.class.getResource("res/login.png")));
	    loginButton.setToolTipText(res.getString("AdminPanel.Login"));
	    loginButton.addActionListener(new ActionListener() {
			
			@Override
			public void actionPerformed(ActionEvent e) {
				
				Runnable runnable = new Runnable() {
					
					@Override
					public void run() {
						loginButton.setEnabled(false);
						PortalConnector.getUser(true);
						loginButton.setEnabled(true);
					}
				};
				Thread t = new Thread(runnable);
				t.start();
			}
		});
	    toolBar.add(loginButton);
	    
	    logoutButton = new JButton(new ImageIcon(AdministrationPanel.class.getResource("res/logout.png")));
	    logoutButton.setToolTipText(res.getString("AdminPanel.Logout"));
	    logoutButton.addActionListener(new ActionListener() {
			
			@Override
			public void actionPerformed(ActionEvent e) {
				PortalConnector.logout();
			}
		});
	    toolBar.add(logoutButton);
	    checkButtonVisibility();
		
	}

	private void checkButtonVisibility() {
		if (PortalConnector.getUser(false) == null) {
	    	logoutButton.setVisible(false);
	    	loginButton.setVisible(true);
	    } else {
			loginButton.setVisible(false);
			logoutButton.setVisible(true);
		}
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		checkButtonVisibility();
	}
	
	private class SwitchPanelButton extends JButton {
		
		public SwitchPanelButton(final String panelIdentifier, String iconUrl) {
			
			super(new ImageIcon(AdministrationPanel.class.getResource(iconUrl)));
			addActionListener(new ActionListener() {
				
				@Override
				public void actionPerformed(ActionEvent e) {
					CardLayout layout = (CardLayout) mainPanel.getLayout(); 
				    layout.show(mainPanel, panelIdentifier);
				}
			});
		}
	}

}
