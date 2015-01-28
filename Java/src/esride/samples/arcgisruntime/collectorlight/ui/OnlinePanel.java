package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.CardLayout;
import java.awt.Color;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.Font;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.util.ResourceBundle;
import java.util.concurrent.ExecutionException;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.swing.BoxLayout;
import javax.swing.JButton;
import javax.swing.JComponent;
import javax.swing.JLabel;
import javax.swing.JLayeredPane;
import javax.swing.JPanel;
import javax.swing.JProgressBar;
import javax.swing.JScrollPane;
import javax.swing.JTextField;
import javax.swing.JToolBar;
import javax.swing.SwingUtilities;
import javax.swing.SwingWorker;
import javax.swing.border.EmptyBorder;
import javax.swing.border.LineBorder;
import javax.swing.plaf.basic.BasicProgressBarUI;

import esride.samples.arcgisruntime.collectorlight.model.PortalResult;
import esride.samples.arcgisruntime.collectorlight.services.PortalConnector;

public class OnlinePanel extends JPanel {
	
	public static final Font TOP_STATUS_FONT = new Font("Dialog", Font.ITALIC, 13);
	public static final LineBorder UNSELECTED_BORDER = new LineBorder(new Color(0, 0, 0, 0), 3);
	public static final LineBorder SELECTED_BORDER = new LineBorder(new Color(180, 180, 180), 3);
	public static final Cursor SELECTED_CURSOR = new Cursor(Cursor.HAND_CURSOR);
	
	private static Logger logger = Logger.getLogger(OnlinePanel.class.getName());
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	
	private JTextField textSearchField;
	private BrowseMapsPanel browseMapsPanel;
	private JPanel _mainPanel; 
	private JLabel searchStatusLabel;
	private JProgressBar progressBar; 
	private JScrollPane scrollPaneSearch; 
	 
	private static final String RESULTS_PANEL = "RESULTS_PANEL"; 
	private static final String PROGRESS_PANEL = "PROGRESS_PANEL"; 
	protected static final int NUM_RESULTS_PER_PAGE = 50;
	
	public OnlinePanel() {
		createUI();
	}

	private void createUI() {
		setLayout(new BorderLayout());
		addToolbar();
		addMainPanel();
	}
	
	private void addMainPanel() {
	 
	    // search panel 
	    JPanel searchStatus = new JPanel(); 
	    searchStatus.setBackground(Color.DARK_GRAY); 
	    searchStatus.setLayout(new BoxLayout(searchStatus, BoxLayout.LINE_AXIS)); 
	    searchStatus.setBorder(new EmptyBorder(5, 10, 5, 10)); 
	    searchStatusLabel = new JLabel(" "); 
	    searchStatusLabel.setOpaque(true); 
	    searchStatusLabel.setForeground(Color.WHITE); 
	    searchStatusLabel.setBackground(Color.DARK_GRAY); 
	    searchStatusLabel.setFont(TOP_STATUS_FONT); 
	    searchStatus.add(searchStatusLabel); 
	 
	    browseMapsPanel = new BrowseMapsPanel(true);
	    scrollPaneSearch = new JScrollPane(browseMapsPanel); 
	    scrollPaneSearch.setBorder(null); 
	 
	    JPanel statusAndSearchPanel = new JPanel(); 
	    statusAndSearchPanel.setLayout(new BorderLayout(0, 1)); 
	    statusAndSearchPanel.add(scrollPaneSearch, BorderLayout.CENTER); 
	    statusAndSearchPanel.add(searchStatus, BorderLayout.NORTH); 
	 
	    // to show a progress bar when a search is being performed 
	    JLayeredPane progressContainer = new JLayeredPane(); 
	    progressContainer.setLayout(new BorderLayout()); 
	    JPanel dummyPanel = new JPanel(); 
	    Dimension preferredSize = new Dimension(1000, 700); 
	    dummyPanel.setBackground(Color.WHITE); 
	    dummyPanel.setPreferredSize(preferredSize); 
	 
	    progressBar = createProgressBar(dummyPanel); 
	    progressContainer.add(progressBar); 
	    progressContainer.add(dummyPanel); 
	 
	    JPanel progressPanel = new JPanel(); 
	    progressPanel.setLayout(new BorderLayout(0, 1)); 
	    progressPanel.add(progressContainer, BorderLayout.CENTER); 
	 
	    // put panels in card layout panel 
	    _mainPanel = new JPanel(); 
	    _mainPanel.setLayout(new CardLayout(0, 0)); 
	    _mainPanel.add(statusAndSearchPanel, RESULTS_PANEL);
	    _mainPanel.add(progressPanel, PROGRESS_PANEL);
	    add(_mainPanel, BorderLayout.CENTER);
		
	}

	private void addToolbar() {
		JToolBar toolBar = new JToolBar();
		toolBar.setFloatable(false);
	    add(toolBar, BorderLayout.NORTH); 
	     
	    JLabel searchInstructions = new JLabel(res.getString("OverviewPanel.searchLabel.Text")); 
	    toolBar.add(searchInstructions); 
	 
	    // Add search text box to toolbar and set it to handle the 
	    // enter key by performing a search. 
	    textSearchField = new JTextField(); 
	    toolBar.add(textSearchField); 
	    textSearchField.setColumns(10); 
	    textSearchField.addActionListener(new ActionListener() { 
	 
			@Override 
			public void actionPerformed(ActionEvent e) { 
				performQuery(textSearchField.getText()); 
			} 
	    }); 
	 
	    // Add a search button to the toolbar. 
	    JButton btnSearch = new JButton(res.getString("OverviewPanel.searchButton.Text")); 
	    btnSearch.addActionListener(new ActionListener() { 
	 
			@Override 
			public void actionPerformed(ActionEvent e) { 
				performQuery(textSearchField.getText()); 
			} 
	    }); 
	    toolBar.add(btnSearch);
	}
	
	 /** 
	   * Creates a progress bar. 
	   * @param parent progress bar's parent. The horizontal axis of the progress bar will be 
	   * center-aligned to the parent. 
	   * @return a progress bar. 
	   */ 
	  private static JProgressBar createProgressBar(final JComponent parent) { 
	    final JProgressBar progressBar = new JProgressBar(); 
	    progressBar.setSize(400, 25); 
	    progressBar.setBackground(Color.DARK_GRAY); 
	    progressBar.setBorderPainted(false); 
	    progressBar.setForeground(Color.WHITE); 
	    progressBar.setUI(new BasicProgressBarUI() { 
	      @Override 
	      protected Color getSelectionBackground() { return Color.WHITE; } 
	      @Override 
	      protected Color getSelectionForeground() { return Color.DARK_GRAY; } 
	    }); 
	    parent.addComponentListener(new ComponentAdapter() { 
	      @Override 
	      public void componentResized(ComponentEvent e) { 
	        progressBar.setLocation( 
	            parent.getWidth()/2 - progressBar.getWidth()/2, 
	            parent.getHeight()/2 - progressBar.getHeight()/2 - 20); 
	      } 
	    }); 
	    progressBar.setStringPainted(true); 
	    progressBar.setIndeterminate(true); 
	    progressBar.setVisible(false); 
	    return progressBar; 
	  }
	  
	  private void showProgressPanel(String progress) {
		  CardLayout layout = (CardLayout) _mainPanel.getLayout(); 
		  updateProgressBarUI(progress, true); 
		  layout.show(_mainPanel, PROGRESS_PANEL);
	  }
	  
	  /** 
	   * Updates progress bar UI from Swing's Event Dispatch Thread. 
	   * @param str string to be set. 
	   * @param visible flag to indicate visibility of the progress bar. 
	   */ 
	  private void updateProgressBarUI(final String str, final boolean visible) { 
	    SwingUtilities.invokeLater(new Runnable() { 
	      @Override 
	      public void run() { 
	        if (str != null) { 
	          progressBar.setString(str); 
	        } 
	        progressBar.setVisible(visible); 
	      } 
	    });  
	  }
	  
	  /** 
	   * Searches for items on the portal that match the given query string. This is 
	   * run as a background task inside a ProgressDialog which will be displayed 
	   * until the query completes. 
	   *  
	   * @param queryString 
	   */ 
	  private void performQuery(final String queryString) {
		  String message = res.getString("OverviewPanel.PerformingQuery.Message");
		  String progressMessage = message + queryString;
		  showProgressPanel(progressMessage); 
	 
		  browseMapsPanel.removeAll(); 
		  browseMapsPanel.repaint(); 
		  browseMapsPanel.revalidate();
		  
		  QuerySwingWorker worker = new QuerySwingWorker(queryString);
		  worker.execute(); 
	  }

	/** 
	 * Displays the search results and disables the back button. 
	 */ 
	private void showResultsPanel(String statusText) { 
	    CardLayout layout = (CardLayout) _mainPanel.getLayout(); 
	    layout.show(_mainPanel, RESULTS_PANEL); 
	    if (statusText != null) { 
	      searchStatusLabel.setText(statusText); 
	    }
	    
	    SwingUtilities.invokeLater(new Runnable() { 
	      @Override 
	      public void run() { 
	        scrollPaneSearch.getVerticalScrollBar().setValue(0); 
	      } 
	    });
	}
	
	private class QuerySwingWorker extends SwingWorker<PortalResult, Void> {
		
		private PortalResult results;
		private String queryString;
		
		public QuerySwingWorker(String queryString) {
			this.queryString = queryString;
		}

		@Override 
	      public PortalResult doInBackground() { 
	        
	        return PortalConnector.queryWebMaps(queryString, NUM_RESULTS_PER_PAGE); 
	      } 
	 
	      @Override 
	      public void done() { 
	        try { 
	          results = get(); 
	        } catch (InterruptedException e1) { 
	        	logger.log(Level.SEVERE, "Fehler in Query", e1);
	        } catch (ExecutionException e1) { 
	        	logger.log(Level.SEVERE, "Fehler in Query", e1);
	        } 
	        if (results == null) {
	        	
	          showResultsPanel(res.getString("OverviewPanel.PerformingQuery.NoResult")); 
	        } else {
	          String mes = res.getString("OverviewPanel.PerformingQuery.Result");
	          int numbers = results.getNumResults() > NUM_RESULTS_PER_PAGE ? NUM_RESULTS_PER_PAGE : results.getNumResults();
	          showResultsPanel(String.format(mes, numbers, results.getNumResults(), queryString));
	        } 
	      }
		
	}
}
