using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class LinkPreviewBatchResultResponse
    {
        public IEnumerable<LinkPreviewResponse> SucceededRequests { get; set; }

        public IEnumerable<LinkPreviewResponse> FailedRequests { get; set; }

    }
}
