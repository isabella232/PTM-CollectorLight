package esride.samples.arcgisruntime.collectorlight.services;

import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Calendar;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

import com.esri.core.portal.LicenseInfo;
import com.esri.core.portal.Portal;
import com.esri.runtime.ArcGISRuntime;

public class LicenseManager {
	
	static final String appId = CollectorConfig.getConfiguration().getProperty("PortalConnector.appId", "");
	static final String pathLicenseFile = CollectorConfig.getConfiguration().getProperty("LicenseManager.pathLicenseFile", "data/lisenceFile");
	private static Logger logger = Logger.getLogger(LicenseManager.class.getName());
	
	private LicenseManager() {
	}
	
	public static boolean licenseArcGISRuntime() {
		
		// Basic Lizenz
		ArcGISRuntime.setClientID(appId);
    	
		// Standard Lizenz
    	try {
    		
    		// Lizenzinfo aus Datei lesen, falls eine vorhanden.
	    	// Diese darf nicht älter als 30 Tage, da der Lizenzstring dann abläuft.
	    	Path licensePath = Paths.get(pathLicenseFile);
	    	Calendar expireDate = Calendar.getInstance();
	    	expireDate.add(Calendar.DAY_OF_YEAR, 29);
	    	if (Files.exists(licensePath) && Files.getLastModifiedTime(licensePath).toMillis() < expireDate.getTimeInMillis()) {
				List<String> lines = Files.readAllLines(licensePath, StandardCharsets.ISO_8859_1);
				if (lines != null && lines.size() > 0) {
					LicenseInfo licenseInfo = LicenseInfo.fromJson(lines.get(0));
					// Lizenz setzen
					ArcGISRuntime.License.setLicense(licenseInfo);
					return true;
				}
			}
		} catch (IOException e) {
			logger.log(Level.SEVERE, "Lizenzdatei konnte nicht gelesen werden", e);
		}
    	
    	// Lizenzinfo mit Hilfe des Portals generieren
    	Portal portal = PortalConnector.getPortal(true);
		LicenseInfo licenseInfo = null;
		try {
			licenseInfo = portal.fetchPortalInfo().getLicenseInfo();
		} catch (Exception e) {
			logger.log(Level.SEVERE, "Lizenzstring konnte nicht geladen werden", e);
			return false;
		}
		if (licenseInfo == null) {
			logger.log(Level.SEVERE, "Lizenzstring konnte nicht geladen werden");
			return false;
		}		

		// Lizenzinfo in lokale Datei schreiben, um diese beim nächsten Start der Anwendung zu verwenden.
		// Der Lizenzstring ist 30 Tage gültig, dann muss er wieder erneuert werden.
		try {
			String licenseJson = licenseInfo.toJson();
			Path licensePath = Paths.get(pathLicenseFile);
			Files.deleteIfExists(licensePath);
			Files.createFile(licensePath);
			Files.write(licensePath, licenseJson.getBytes());
		} catch (IOException e) {
			logger.log(Level.SEVERE, "Lizenzdatei konnte nicht gelesen werden", e);
		}

		// Lizenz setzen
		ArcGISRuntime.License.setLicense(licenseInfo);
		return true;
	}

}
