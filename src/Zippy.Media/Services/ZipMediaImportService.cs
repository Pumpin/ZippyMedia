using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Zippy.Media.Services
{
    public class ZipMediaImportService  
    {
        private const string UnZipTempRoot = "~/App_Data/Unzipped/";
        private Dictionary<string, string> Mappings = new Dictionary<string, string>()
        {
            {".jpg", "Image"},
            {".jpeg", "Image"},
            {".png", "Image"},
            {".bmp", "Image"},
            {".gif", "Image"},
            {".tif", "Image"},
            {".tiff", "Image"}
        };

        private Dictionary<string, string> LoadCustomMappings()
        {
            var section = (Hashtable)ConfigurationManager.GetSection("ZippyMedia");
            return section.Cast<DictionaryEntry>().ToDictionary(d => (string)d.Key, d => (string)d.Value);
        }

        public void ImportMedia(string unzipFolder, int importFolderId)
        {
            //Load up mappings from config file
            foreach (var customMapping in LoadCustomMappings())
            {
                if(!Mappings.ContainsKey(customMapping.Key))
                    Mappings.Add(customMapping.Key, customMapping.Value);
            }

            var root = HttpContext.Current.Server.MapPath(UnZipTempRoot + unzipFolder);
            TraverseAndCreate(root, importFolderId);
        }

        private void TraverseAndCreate(string path, int mediaRoot)
        {
            var mediaService = UmbracoContext.Current.Application.Services.MediaService;

            try
            {
                foreach (string dir in Directory.GetDirectories(path))
                {
                    var directoryInfo = new DirectoryInfo(dir);
                    var newMediaRoot = mediaService.CreateMedia(directoryInfo.Name, mediaRoot, "Folder");
                    mediaService.Save(newMediaRoot);

                    TraverseAndCreate(dir, newMediaRoot.Id);
                }

                var mediaList = new List<IMedia>();
                
                foreach (string file in Directory.GetFiles(path))
                {
                    using (var fileStream = new FileStream(file, FileMode.Open))
                    {
                        var fileInfo = new FileInfo(file);
                        
                        if (UmbracoConfig.For.UmbracoSettings().Content.DisallowedUploadFiles.Contains(fileInfo.Extension.Replace(".", "")))
                        {
                            //file is not allowed
                            continue;
                        }

                        var mediaType = "File"; //default 
                        if (Mappings.ContainsKey(fileInfo.Extension.ToLower()))
                            mediaType = Mappings[fileInfo.Extension];

                        var createdMedia = mediaService.CreateMedia(fileInfo.Name, mediaRoot, mediaType);
                        createdMedia.SetValue("umbracoFile", fileInfo.Name, fileStream);
                        mediaList.Add(createdMedia);
                        mediaService.Save(createdMedia);

                    }
                    
                }
               

            }
            catch (Exception exception)
            {
                LogHelper.Error<ZipMediaImportService>("Error uploading and unpacking zip file", exception);
            }
        }



        public string UnzipFile(string zipfilePath, string unzipDirPath)
        {
            string root = HttpContext.Current.Server.MapPath(UnZipTempRoot + unzipDirPath);
            Directory.CreateDirectory(root);

            using (var stream = new ZipInputStream(System.IO.File.OpenRead(zipfilePath)))
            {
                ZipEntry entry;
                while ((entry = stream.GetNextEntry()) != null)
                {

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(root + entry.Name);
                    }

                    if (entry.IsFile)
                    {
                        try
                        {
                            string fileName = root + entry.Name;
                            using (FileStream streamWriter = System.IO.File.Create(fileName))
                            {

                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = stream.Read(data, 0, data.Length);
                                    if (size > 0)
                                    {
                                        streamWriter.Write(data, 0, size);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        catch (IOException exception)
                        {
                            LogHelper.Error<ZipMediaImportService>("Error creating zipfile: ", exception);
                        }
                        
                    }
                }
            }
            return root;
        }

        
    }
}