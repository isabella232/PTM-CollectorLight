package esride.samples.arcgisruntime.collectorlight.services;

import java.beans.PropertyChangeListener;
import java.beans.PropertyChangeSupport;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.esri.core.io.UserCredentials;
import com.esri.core.map.CallbackListener;
import com.esri.core.portal.Portal;
import com.esri.core.portal.PortalItem;
import com.esri.core.portal.PortalItemType;
import com.esri.core.portal.PortalQueryParams;
import com.esri.core.portal.PortalQueryResultSet;

import esride.samples.arcgisruntime.collectorlight.model.MapItem;
import esride.samples.arcgisruntime.collectorlight.model.MapItemList;
import esride.samples.arcgisruntime.collectorlight.model.PortalResult;
import esride.samples.arcgisruntime.collectorlight.ui.OAuthSwingBrowser;
import esride.samples.arcgisruntime.collectorlight.ui.OAuthSwingBrowser.AuthCallbackListener;

public class PortalConnector {
	
	static final String portalUrl = CollectorConfig.getConfiguration().getProperty(
			"PortalConnector.portalUrl", "http://arcgis.com/");
	static final String appId = CollectorConfig.getConfiguration().getProperty("App.appId", "");
	private static Portal PORTAL;
	private static Logger logger = Logger.getLogger(PortalConnector.class.getName());
	private static final PropertyChangeSupport support = new PropertyChangeSupport(new Portal(portalUrl, null));
	public static final String PROP_USER = "userChanged";
	
	private PortalConnector() {
	}
	
	/**
	 * Gibt die aktuelle PortalInstanz zurück.
	 * Ist login true wird zusätzlich die Login-Seite aufgerufen und Benutzername/Passwort abgefragt.
	 * @param login Benutzerlogin durchführen?
	 * @return Portalinstanz oder NULL wenn Login nicht erfolgreich war
	 */
	public static Portal getPortal(boolean login) {
		if (PORTAL != null)
			return PORTAL;

		if (login) {
			authorize();
			if (PORTAL != null && PORTAL.getCredentials() != null) {
				support.firePropertyChange(PROP_USER, null, PORTAL.getCredentials().getUserName());
			}
			return PORTAL;
		}
		return new Portal(portalUrl, null);
	}
	
	/**
	 * Gibt den aktuell an der Portal-Instanz angemeldeten Benutzer zurück.
	 * Ist noch kein Nutzer angemeldet und login true wird die Login-Seite aufgerufen und Benutzername/Passwort abgefragt.
	 * Ist noch kein Nutzer angemeldet und login false wird NULL zurück gegeben.
	 * @param login Benutzerlogin durchführen?
	 * @return Aktueller Benutzer oder NULL
	 */
	public static UserCredentials getUser(boolean login) {
		if (login)
			getPortal(true);
		
		if (PORTAL != null) {
			return PORTAL.getCredentials();
		}
		
		return null;
	}
	
	public static void addListener(final PropertyChangeListener listener) {
		support.addPropertyChangeListener(listener);
	}
	
	private static void authorize () {
		
		final Portal portal = new Portal(portalUrl, null);
		
		// 1. Callback. Diesem wird eine generierte URL übergeben, um den Authorization Code zu erzeugen.
		Portal.GetAuthCodeCallback authcodeCallback = new Portal.GetAuthCodeCallback() {
			String authorizationCode = "";
			
			@Override
			public String getAuthcode(final String authorizationUrl) {

				final CountDownLatch latch = new CountDownLatch(1);
				final OAuthSwingBrowser browser = new OAuthSwingBrowser();
				
				browser.loadAuthCode(authorizationUrl, new AuthCallbackListener() {
					@Override
					public void onCallback(String authCode) {
						authorizationCode = authCode;
						latch.countDown();
					}
				});
				
				try { // Auf OAuthSwingBrowser warten
			      latch.await();
				} catch (InterruptedException e) {
					logger.log(Level.SEVERE, "Authorization failed", e);
				}

				return authorizationCode;
			}
		};
		
		// 2. Callback. Wird aufgerufen wenn Authentifizierung abgeschlossen
		final CountDownLatch latch = new CountDownLatch(1);
		CallbackListener<Portal> authorizedCallback = new CallbackListener<Portal>() {
			
			@Override
			public void onError(Throwable e) {
				logger.log(Level.SEVERE, "Authorization failed", e);
				PORTAL = portal;
				latch.countDown();
			}
			
			@Override
			public void onCallback(Portal objs) {
				PORTAL = objs;
				latch.countDown();
			}
		};
		
		try {
			// Login starten
			portal.doOAuthUserAuthenticate(null, appId, authcodeCallback, authorizedCallback);
			latch.await(); // Auf Authentifizierung Callback warten
		} catch (Exception e1) {
			logger.log(Level.SEVERE, "Authorization failed", e1);
		}
	}
	
	public static PortalResult queryWebMaps(String queryString, int limit) {
		PortalQueryParams queryParams = new PortalQueryParams(); 
        // query for web maps 
        queryParams.setQuery(PortalItemType.WEBMAP, null, queryString); 
        // max number of results to return 
        queryParams.setLimit(limit); // default is 10 
 
        PortalResult myResult = new PortalResult(); 
        try { 
          PortalQueryResultSet<PortalItem> portalItems = getPortal(false).findItems(queryParams);
          List<MapItem> myItems = new ArrayList<MapItem>();
          for (PortalItem portalItem : portalItems.getResults()) {
        	  myItems.add(new MapItem(portalItem, true));
          }
          myResult.setItems(myItems); 
          myResult.setNumResults(portalItems.getTotalResults());
          MapItemList.getOnlineList().setItemList(myItems);
        } catch (Exception e) {
          logger.log(Level.SEVERE, "Fehler in Query", e);
        } 
        return myResult;
	}
	
	public static void logout() {
		PORTAL = null;
		support.firePropertyChange(PROP_USER, null, null);
	}

}
