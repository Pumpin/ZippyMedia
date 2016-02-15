# Zippy Media Exporters for older versions of Umbraco 
This is not a part of the package, it is developer tools that can assist in exporting media libs from older solutions to a clean and simple folder struckture. 

Some C# knowledge is required.

#### Warning: Here be small dragons. 
Currently there is only a exporter for version 4. The exporter works directly on on the umbraco database instead of the old media api. there for it is best that you upgrade to version 4.11 to get the media struckture in place. (Use umbracos config files for the upgrades cause you will only need the database not a "working solution" :))

Once upgraded dump the file ZippyMediaExporterV4.cs in your project where you can instantiate it from a web context. (It will try log to a HttpContext)

The Exporter will recursive look up the media items in your umbraco database starting from the root node (-1). It will try to use the names from the media libary as filenames for easy import later on.  

#### Example.
basic :
	
    var myExporterV4 = new ZippyMediaExporterV4();
    myExporterV4.ConnString = "Your constring here";
    myExporterV4.Export();

you can override the default folder it exports to "ZippyExportedMediaLib" in the Export method.

#### Usage ?
if you are rebuilding a old complex solution, and you want the media lib from the old should be in the new solution to.  


#### Remember to backup
#### The script only select in the database, so it is free to test run!