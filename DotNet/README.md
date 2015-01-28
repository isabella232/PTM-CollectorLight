# **Collector Light .NET**
Dieses Repository beinhaltet den Sourcecode einer Beispielanwendung auf Basis des ArcGIS Runtime SDK for .NET/Desktop. Es wird die Realisierung einiger Kernfunktionen des [Collector for ArcGIS](http://doc.arcgis.com/de/collector/ "") in einem weiteren ArcGIS Runtime SDK demonstriert.

#![Startcreen.png](StartScreen.png "")


## **Features**

* [2 Faktor Authentifizierung gegenüber ArcGIS Online](https://developers.arcgis.com/net/desktop/guide/use-oauth-2-0-authentication.htm "")
* Suchen von WebMaps auf ArcGIS Online
* Download von Basemaps und FeatureLayern
* Online und Offline Editierung
* Synchronisation von Änderungen



## **Instructions**
##### **Getting Started**

1.     Forken des Repositories
2.     Herunterladen und Installieren des ArcGIS Runtime SDK for .NET/Desktop ([ArcGIS Developer Account](https://developers.arcgis.com/en/sign-up/ "") wird benötigt)
3.     Herunterladen des MVVM Light Toolkit von Laurent Bugnion
4.     Herunterladen von Json.Net von NewtonSoft
5.     Projekt in Visual Studio laden (siehe Requirements), Referenzen setzen und kompilieren

##### **Anwenden der App**

1.     [Registrieren der Anwendung](https://developers.arcgis.com/net/desktop/guide/license-your-app.htm "")  in einer ArcGIS Online Subskription oder in der eigenen Developer Subskription
2. Generierte Client Id der App mitteilen (wird in Settings gespeichert)
2. Mit Button "Anmelden"  an einer Subskription mit Named User oder Developer Account anmelden
3. Mit "Online WebMaps" nach [editierbaren WebMaps](http://doc.arcgis.com/de/collector/android/create-maps/create-and-share-a-collector-map.htm "") in der Subskription suchen
4. In den gefundenen Items die Optionen nutzen für:
    * Karte online editieren und synchronisieren oder 
    * Teilbereiche der Karte als offline Daten exportieren, offline nutzen "Offline Maps" und anschließend synchronisieren

## **Requirements**

* Unterstütze System-Konfiguration für: 
  * [Windows Destkop](http://developers.arcgis.com/net/desktop/guide/system-requirements.htm)


## **Resources**

* [ArcGIS Runtime SDK for .NET](https://developers.arcgis.com/net/)
* [MVVM Light Toolkit von Laurent Bugnion](http://mvvmlight.codeplex.com/ "") (Lizenzbestimmungen beachten)
* [Json.Net von NewtonSoft](https://json.codeplex.com/ "") (Lizenzbestimmungen beachten)

* Zusätzliche [Toolkits](https://github.com/Esri/arcgis-toolkit-dotnet "") und [Samples](https://developers.arcgis.com/net/sample-code/ "") für/basierend auf ArcGIS Runtime SDK for .NET

## **Issues**

Die Entwicklung der Demo begann bereits basierend auf einer frühen ArcGIS Runtime SDK for .NET Beta Version. Seit dieser sind einige neue Versionen erschienen, die zahlreiche wertvolle Änderungen mitgebracht haben. Es gibt mittlerweile sicher für einige Funktionen im Code andere Lösungsansätze.

Bei dem Code handelt es sich um Democode, der nicht vollständig ausprogrammiert und getestet ist. Es können Fehler auftreten, die nicht abgefangen sind.


## **Licensing**
Copyright 2014 Esri Deutschland GmbH

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's LICENSE-2.0.txt file.

[](Esri Tags: ArcGIS Runtime SDK Windows Desktop C-Sharp C# XAML Collector)
[](Esri Language: DotNet)
