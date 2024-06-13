using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class LinkInfo

    {
        public LinkInfo(Uri? url) => this.Url = url;

        public string Title { get; set; }
        public string Description { get; set; }
        public Uri ImageUrl { get; set; }
        public Uri? Url { get; set; }
        public Uri CanoncialUrl { get; set; }
    }
}
