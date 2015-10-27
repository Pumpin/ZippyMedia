using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Zippy.Media.Models
{
    public class ZippyMediaFileModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public string Size { get; set; }
    }
}