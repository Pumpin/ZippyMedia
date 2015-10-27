using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zippy.Media.Models
{
    public class ZippyMediaGetFilesResponse
    {
        public List<ZippyMediaFileModel> Files { get; set; }

        public ZippyMediaGetFilesResponse()
        {
            Files = new List<ZippyMediaFileModel>();
        }
    }
}