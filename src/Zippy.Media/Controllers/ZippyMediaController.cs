using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Zippy.Media.Models;
using Zippy.Media.Services;

namespace Zippy.Media.Controllers
{
    [PluginController("ZippyMedia")]
    public class ZippyMediaController : UmbracoAuthorizedApiController
    {
        public async Task<HttpResponseMessage> UploadFiles()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = HttpContext.Current.Server.MapPath("~/App_Data/ZipUploads");
            Directory.CreateDirectory(root);

            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                var response = new List<ZippyMediaUploadResponse>();
                var result = await Request.Content.ReadAsMultipartAsync(provider);
                
                var startFolderId = result.FormData["startFolderId"];
                if (startFolderId == null)
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                foreach (MultipartFileData file in result.FileData)
                {
                    var fileNameWithExtension = file.Headers.ContentDisposition.FileName.Trim('"');
                    var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);

                    var mediaImporter = new ZipMediaImportService();
                    var unzipRoot = mediaImporter.UnzipFile(file.LocalFileName, fileName + "\\");

                    mediaImporter.ImportMedia(fileName, int.Parse(startFolderId));


                    response.Add(new ZippyMediaUploadResponse(fileName, unzipRoot));
                    CleanUploadedTempFile(file.LocalFileName);
                    CleanUnzippedTempFile(unzipRoot);
                }

                Services.MediaService.RebuildXmlStructures();
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exception)
            {
                LogHelper.Error<ZippyMediaController>("Error upload zip files", exception);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception);
            }
        }
        private void CleanUnzippedTempFile(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                dirInfo.Delete(true);
            }
            catch (Exception exception)
            {
                LogHelper.Error<ZippyMediaController>("Error Cleaning up unzipped temp files", exception);
            }
            
        }
        private void CleanUploadedTempFile(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                fileInfo.Delete();
            }
            catch (Exception exception)
            {
                LogHelper.Error<ZippyMediaController>("Error Cleaning up uploaded temp files", exception);
            }
        }

        /// <summary>
        /// Gets the files from server. Not Implemented yet.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetFilesFromServer()
        {
            var response = new ZippyMediaGetFilesResponse();
            var root = HttpContext.Current.Server.MapPath("~/App_Data/ZipUploads");

            try
            {
                foreach (string file in Directory.GetFiles(root))
                {
                    var fileInfo = new FileInfo(file);
                    response.Files.Add(new ZippyMediaFileModel
                    {
                        Name = fileInfo.Name,
                        Path = fileInfo.FullName,
                        Extension = fileInfo.Extension
                    });
                }
            }
            catch (Exception exception)
            {

                throw;
            }


            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}