package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.util.ResourceBundle;

import javax.swing.JFrame;
import javax.swing.JScrollPane;
import javax.swing.JTabbedPane;
import javax.swing.JTextArea;

import esride.samples.arcgisruntime.collectorlight.services.MessageManager;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;

public class CollectorFrame extends JFrame implements PropertyChangeListener {
	
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	MapPanel mapPanel;
	JTextArea statusTextArea;
	
	public CollectorFrame() {
		setTitle(res.getString("CollectorFrame.Title"));
		createUI();
		MessageManager.addListener(this);
		addWindowListener(new WindowAdapter() {
			@Override
			public void windowClosing(WindowEvent e) {
				super.windowClosing(e);
				if (ProjectManager.getCurrentProject() != null)
					ProjectManager.getCurrentProject().dispose();
				mapPanel.dispose();
			}
		});
	}

	private void createUI() {
		getContentPane().setLayout(new BorderLayout());
		addMainPanel();
		addStatusTextArea();
	}
	
	private void addMainPanel() {
		JTabbedPane mainPanel = new JTabbedPane();
		mainPanel.setSize(1000, 700);
		AdministrationPanel adminPanel = new AdministrationPanel();
		mapPanel = new MapPanel();
		mainPanel.add(res.getString("CollectorFrame.overviewPanel.Title"), adminPanel);
		mainPanel.add(res.getString("CollectorFrame.mapPanel.Title"), mapPanel);
		getContentPane().add(mainPanel, BorderLayout.CENTER);
	}

	private void addStatusTextArea() {
		statusTextArea = new JTextArea(3, 1);
		statusTextArea.setEditable(false);
		JScrollPane scrollPane = new JScrollPane(statusTextArea);
		scrollPane.setAutoscrolls(true);
		getContentPane().add(scrollPane, BorderLayout.SOUTH);
	}

	@Override
	public void propertyChange(PropertyChangeEvent evt) {
		if (evt.getPropertyName().equals(MessageManager.TEXT_MESSAGE)) {
			statusTextArea.setText(evt.getNewValue() + "\n" + statusTextArea.getText());
		} else if (evt.getPropertyName().equals(MessageManager.TEXT_VISIBLE)) {
			statusTextArea.setVisible((boolean) evt.getNewValue());
		}
	}

}
