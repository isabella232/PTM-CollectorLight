/* Copyright 2014 Esri

All rights reserved under the copyright laws of the United States
and applicable international laws, treaties, and conventions.

You may freely redistribute and use this sample code, with or
without modification, provided you include the original copyright
notice and use restrictions.

See the use restrictions.*/
package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Component;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.Font;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.util.ResourceBundle;
import java.util.logging.Level;
import java.util.logging.Logger;

import javax.imageio.ImageIO;
import javax.swing.BoxLayout;
import javax.swing.ImageIcon;
import javax.swing.JLabel;
import javax.swing.JPanel;
import javax.swing.JTextArea;
import javax.swing.border.EmptyBorder;
import javax.swing.border.LineBorder;

import esride.samples.arcgisruntime.collectorlight.model.MapItem;
import esride.samples.arcgisruntime.collectorlight.services.ProjectManager;

/**
 * Create a panel that will display a thumbnail along with its description.
 *
 */
public class PortalItemPanel extends JPanel {
	
	public static final Color FOREGROUND_COLOR = Color.DARK_GRAY;
	public static final Color FOREGROUND_LIGHTER_COLOR = new Color(100, 100, 100);
	public static final Color BG_COLOR = Color.WHITE;
	public static final Font TITLE_FONT = new Font("Dialog", Font.BOLD, 14);
	public static final Cursor SELECTED_CURSOR = new Cursor(Cursor.HAND_CURSOR);
	public static final Cursor DEFAULT_CURSOR = Cursor.getDefaultCursor();
	public static final Font DESCRIPTION_FONT = new Font("Dialog", Font.PLAIN, 12);
	public static final Font OWNER_FONT = new Font("Dialog", Font.ITALIC, 12);
	public static final Font TOP_STATUS_FONT = new Font("Dialog", Font.ITALIC, 13);
	public static final LineBorder UNSELECTED_BORDER = new LineBorder(new Color(0, 0, 0, 0), 3);
	public static final LineBorder SELECTED_BORDER = new LineBorder(new Color(180, 180, 180), 3);
	public static final int DESC_WIDTH = 350;
	public static final int TH_WIDTH = 200 + 2;
	public static final int TH_HEIGHT = 133 + 2;
	public static final int TITLE_HEIGHT = 30;
	public static final int OWNER_HEIGHT = 20;
	
	private static ResourceBundle res = ResourceBundle.getBundle("res.ui");
	private static Logger logger = Logger.getLogger(PortalItemPanel.class.getName());
	private MapItem _item;
	private JTextArea txtSnippetArea;

  private static final long serialVersionUID = 1L;

  /**
   * Creates a portal item panel based on a portal item, using title, description, 
   * and thumbnail.
   * 
   * @param item
   */
  public PortalItemPanel (MapItem item) {
    _item = item;

    setLayout(new java.awt.BorderLayout(2, 2));
    setBackground(BG_COLOR);
    setForeground(FOREGROUND_COLOR);
    setBorder(UNSELECTED_BORDER);

    String title = item.getTitle();
    String description = "";
    if (item.getSnippet() == null) { 
      description = res.getString("PortalItemPanel.MissingDescription");
    } else if (item.getSnippet().isEmpty() || item.getSnippet().length() < 6) {
      description = res.getString("PortalItemPanel.MissingDescription");
    } else {
      description = item.getSnippet().trim();
    }

    // title
    String titleForLabel = title;
    if (title == null) {
    	titleForLabel = "...";
    } else if (title.length() > 45) {
      titleForLabel = title.substring(0, 42) + "...";
    }
    final JLabel labelTitle = new JLabel(titleForLabel);
    labelTitle.setForeground(FOREGROUND_COLOR);
    labelTitle.setFont(TITLE_FONT);
    labelTitle.setAlignmentX(Component.LEFT_ALIGNMENT);
    labelTitle.setOpaque(false);
    JPanel titlePanel = new JPanel();
    titlePanel.setLayout(new FlowLayout(FlowLayout.LEFT));
    titlePanel.setBackground(BG_COLOR);
    titlePanel.setAlignmentX(Component.LEFT_ALIGNMENT);
    Dimension titleDim = new Dimension(DESC_WIDTH, TITLE_HEIGHT);
    titlePanel.setPreferredSize(titleDim);
    titlePanel.setMaximumSize(titleDim);
    titlePanel.add(labelTitle);
    

    // thumbnail
    JLabel thumbnail = new JLabel(res.getString("PortalItemPanel.MissingThumbnail"));
    if (item.getIcon() != null) {
        try {
        	ByteArrayOutputStream baos = new ByteArrayOutputStream();
			ImageIO.write(item.getIcon(), "PNG", baos);
			ImageIcon icon = new ImageIcon(baos.toByteArray());
	    	thumbnail = new JLabel(icon);
		} catch (IOException e) {
			// TODO Auto-generated catch block
			logger.log(Level.SEVERE, "Thumbnail konnte nicht geladen werden", e);
		}
    }
    thumbnail.setForeground(Color.DARK_GRAY);
    thumbnail.setBackground(Color.WHITE);
    thumbnail.setAlignmentY(Component.TOP_ALIGNMENT);
    thumbnail.setAlignmentX(Component.LEFT_ALIGNMENT);
    thumbnail.setBorder(new LineBorder(Color.LIGHT_GRAY, 1));

    final JPanel thumbnailPanel = new JPanel();
    thumbnailPanel.setLayout(new FlowLayout(FlowLayout.LEFT, 0, 0));
    thumbnailPanel.setBackground(BG_COLOR);
    thumbnailPanel.setForeground(FOREGROUND_COLOR);
    Dimension dim = new Dimension(TH_WIDTH, TH_HEIGHT);
    thumbnailPanel.setMaximumSize(dim);
    thumbnailPanel.setMinimumSize(dim);
    thumbnailPanel.setPreferredSize(dim);
    thumbnailPanel.add(thumbnail);
    thumbnailPanel.revalidate();
    
    // description plain text
    txtSnippetArea = new JTextArea(description);
    txtSnippetArea.setFont(DESCRIPTION_FONT);
    txtSnippetArea.setForeground(FOREGROUND_COLOR);
    txtSnippetArea.setBackground(BG_COLOR);
    txtSnippetArea.setWrapStyleWord(true);
    txtSnippetArea.setLineWrap(true);
    txtSnippetArea.setEnabled(false);
    txtSnippetArea.setFocusable(false);
    txtSnippetArea.setDisabledTextColor(FOREGROUND_COLOR);
    txtSnippetArea.setAlignmentY(Component.TOP_ALIGNMENT);
    txtSnippetArea.setAlignmentX(Component.LEFT_ALIGNMENT);
    txtSnippetArea.setBorder(new EmptyBorder(0, 5, 0, 0));
    Dimension descDim = new Dimension(DESC_WIDTH, TH_HEIGHT - TITLE_HEIGHT - OWNER_HEIGHT);
    txtSnippetArea.setPreferredSize(descDim);
    txtSnippetArea.setMaximumSize(descDim);

    String ownerString = item.getOwner();
    String ownerLabelText = null;
    if (ownerString == null || ownerString.isEmpty()) {
      ownerLabelText = "";
    } else {
      ownerLabelText = res.getString("PortalItemPanel.OwnerLabelText") + ownerString;
    }
    JLabel owner = new JLabel(ownerLabelText);
    owner.setBorder(new EmptyBorder(0, 5, 0, 0));
    owner.setForeground(FOREGROUND_LIGHTER_COLOR);
    owner.setFont(OWNER_FONT);
    
    JPanel titleAndDescription = new JPanel();
    titleAndDescription.setBackground(BG_COLOR);
    titleAndDescription.setLayout(new BoxLayout(titleAndDescription, BoxLayout.Y_AXIS));
    titleAndDescription.add(titlePanel);
    titleAndDescription.add(txtSnippetArea);
    titleAndDescription.add(owner);

    add(thumbnailPanel, BorderLayout.WEST);
    add(titleAndDescription, BorderLayout.CENTER);

    revalidate();
    
    addMouseListener();
  }
  
  private void addMouseListener() {
	  MouseAdapter mouseAdapter = new MouseAdapter() { 
          @Override 
          public void mouseClicked(MouseEvent e) {
        	  ProjectManager.openProject(_item, null);
          } 
          @Override 
          public void mouseEntered(MouseEvent e) { 
            setBorder(SELECTED_BORDER); 
            setCursor(SELECTED_CURSOR); 
          } 
          @Override 
          public void mouseExited(MouseEvent e) { 
            setBorder(UNSELECTED_BORDER); 
            setCursor(Cursor.getDefaultCursor()); 
          } 
        }; 
        addMouseListener(mouseAdapter); 
        getTxtSnippetArea().addMouseListener(mouseAdapter); 
  }

/**
   * Get access to the PortalItem this instance represents so we can display
   * it in a click handler later.
   * 
   * @return PortalItem used to create this instance
   */
  public MapItem getItem(){
    return _item;
  }
  
  public JTextArea getTxtSnippetArea() {
    return txtSnippetArea;
  }
}