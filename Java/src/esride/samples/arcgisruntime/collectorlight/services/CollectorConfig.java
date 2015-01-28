package esride.samples.arcgisruntime.collectorlight.services;

import java.io.BufferedReader;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.Properties;
import java.util.logging.Level;
import java.util.logging.Logger;

public class CollectorConfig
{
  private static final Logger logger = Logger.getLogger(CollectorConfig.class.getName());

  public static final String CONFIG_FILENAME = "data/config.properties";

  private static Properties configuration;
  
  private CollectorConfig()
  {
  }


  public static Properties getConfiguration()
  {
	  if (configuration == null)
	  {
		  configuration = loadConfiguration();
	  }
	  return configuration;
  }

	private static Properties loadConfiguration() {
		
		Path configPath = Paths.get(CONFIG_FILENAME);
		if (Files.exists(configPath)) {
			try {
				BufferedReader reader = Files.newBufferedReader(configPath);
				Properties prop = new Properties();
				prop.load(reader);
				return prop;
			} catch (IOException e) {
				logger.log(Level.SEVERE, "Config " + configPath + " konnte nicht geladen werden", e);
			}
		}
		
		return null;
		
	}
	
}
