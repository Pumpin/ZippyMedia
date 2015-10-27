namespace Zippy.Media.Models
{
    public class ZippyMediaUploadResponse
    {
        public string FileName { get; set; }
        public string UnzippedDataPath { get; set; }


        public ZippyMediaUploadResponse()
        {
            
        }

        public ZippyMediaUploadResponse(string fileName, string unzippedDataPath)
        {
            FileName = fileName;
            UnzippedDataPath = unzippedDataPath;
        }
    }
}