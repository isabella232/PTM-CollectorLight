package esride.samples.arcgisruntime.collectorlight.services;

import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;

public class MessageManager {
	
	public static final String TEXT_MESSAGE = "TEXT_MESSAGE";
	public static final String TEXT_VISIBLE = "TEXT_VISIBLE";
	public static final String PROGRESS_MESSAGE = "PROGRESS_MESSAGE";
	public static final String PROGRESS_VISIBLE = "PROGRESS_VISIBLE";
	
	static String textMessage = null;
	static boolean messageVisible = false;
	static String progressMessage = null;
	static boolean progressVisible = false;
	
	private static final PropertyChangeSupport support = new PropertyChangeSupport(new ProjectManager());
	
	public static void addListener(PropertyChangeListener listener) {
		support.addPropertyChangeListener(listener);
	}
	
	public static void addTextMessage(String text) {
		support.firePropertyChange(TEXT_MESSAGE, null, text);
	}
	
	public static void hideMessages() {
		support.firePropertyChange(TEXT_VISIBLE, true, false);
	}
	
	public static void showMessages() {
		support.firePropertyChange(TEXT_VISIBLE, false, true);
	}

	public static void setProgressMessage(String text) {
		support.firePropertyChange(PROGRESS_MESSAGE, null, text);
	}
	
	public static void hideProgress() {
		support.firePropertyChange(PROGRESS_VISIBLE, true, false);
	}
	
	public static void showProgress() {
		support.firePropertyChange(PROGRESS_VISIBLE, false, true);
	}
	
}
