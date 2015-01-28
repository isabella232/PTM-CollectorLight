package esride.samples.arcgisruntime.collectorlight.ui;

import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.Point;
import java.awt.event.MouseEvent;
import java.awt.geom.Rectangle2D;

import com.esri.core.geometry.Envelope;
import com.esri.map.JMap;
import com.esri.map.MapOverlay;

public class SelectExtentOverlay extends MapOverlay {

    private static final long serialVersionUID = 1L;
    private final Color TRANS_BG = new Color(0, 0, 0, 30);
    private Point _firstPoint; // java.awt.Point, in screen coordinates
    private Rectangle2D _zoomRect;
    private Envelope _extent;

    public SelectExtentOverlay(){}

    @Override
    public void onMousePressed(MouseEvent event) {
      // Get first point in screen coords.
      _firstPoint = event.getPoint();
      _zoomRect = new Rectangle2D.Double();
    }

    @Override
    public void onMouseReleased(MouseEvent event) {

      JMap jMap = this.getMap();
      com.esri.core.geometry.Point topLeft = jMap.toMapPoint(
          (int)_zoomRect.getMinX(), (int)_zoomRect.getMinY());
      com.esri.core.geometry.Point bottomRight = jMap.toMapPoint(
          (int)_zoomRect.getMaxX(), (int)_zoomRect.getMaxY());
      Envelope extent = new Envelope(topLeft.getX(), topLeft.getY(),
          bottomRight.getX(), bottomRight.getY());
      _extent = extent;

      _firstPoint = null;
      _zoomRect = null;
    }

    @Override
    public void onMouseDragged(MouseEvent event) {
      if(_firstPoint != null){
        double width = Math.abs(event.getX() - _firstPoint.getX());
        double height = Math.abs(event.getY() - _firstPoint.getY());
        Point topLeft = new Point(); // java.awt.Point
        if(_firstPoint.getX() < event.getX()){
          topLeft.setLocation(_firstPoint.getX(), 0);
        }
        else{
          topLeft.setLocation(event.getX(), 0);
        }

        if(_firstPoint.getY() < event.getY()){
          topLeft.setLocation(topLeft.getX(), _firstPoint.getY());
        }
        else{
          topLeft.setLocation(topLeft.getX(), event.getY());
        }
        _zoomRect.setRect(topLeft.getX(), topLeft.getY(), width, height);
        this.repaint();
      }
    }

    /**
     * Returns the latest user-drawn extent from the overlay.
     * @return an extent Envelope
     */
    public Envelope getExtent() {
      return _extent;
    }

    @Override
    public void paint(Graphics graphics) {
      if(_zoomRect != null){
        Graphics2D g = (Graphics2D) graphics;
        g.setColor(Color.DARK_GRAY);
        g.setStroke(new BasicStroke(2, BasicStroke.CAP_SQUARE,
            BasicStroke.JOIN_MITER, 2, new float[]{6, 6}, 0f));
        g.draw(_zoomRect);
        g.setPaint(TRANS_BG);
        g.fill(_zoomRect);
      }
    }
  }
