package esride.samples.arcgisruntime.collectorlight;

import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

import com.esri.runtime.ArcGISRuntime;
import com.esri.runtime.ArcGISRuntime.RenderEngine;

import esride.samples.arcgisruntime.collectorlight.services.LicenseManager;
import esride.samples.arcgisruntime.collectorlight.ui.CollectorFrame;

public class CollectorApp {

	private static Logger logger = Logger.getLogger(CollectorApp.class.getName());
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
    private JFrame window;

    public CollectorApp() {
    	window = new CollectorFrame();
    }
    
    private static void initializeArcGISRuntime()
	{    	
	    try {
			ArcGISRuntime.setRenderEngine(RenderEngine.DirectX);
			
			try {
				UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
			} catch (ClassNotFoundException | InstantiationException | IllegalAccessException | UnsupportedLookAndFeelException e) {
				Logger.getLogger(CollectorApp.class.getName()).log(Level.SEVERE, "Error while setting LookAndFeel", e);
			}
			JFrame.setDefaultLookAndFeelDecorated(true);
			boolean licensed = LicenseManager.licenseArcGISRuntime();
			if (licensed) {
				openApp();
			} else {
				JOptionPane.showConfirmDialog(null, res.getString("App.noLisence"), "Fehler", JOptionPane.ERROR_MESSAGE);
			}
		} catch (Throwable e) {
			logger.log(Level.SEVERE, "Anwendung konnte nicht lizenziert werden", e);
		}
	}
    
    private static void openApp() {
    	SwingUtilities.invokeLater(new Runnable()
	    {
	    	public void run()
	    	{
				CollectorApp application = new CollectorApp();
				application.window.setBounds(100, 100, 800, 600);
				application.window.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
	        	application.window.setVisible(true);
	    	}
	    });
    }

    /**
     * @param args
     */
    public static void main(String[] args) {
    	initializeArcGISRuntime();
    }
}
