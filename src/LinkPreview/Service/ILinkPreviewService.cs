using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public interface ILinkPreviewService
    {
        Task<LinkPreviewRequest> GetLinkDataAsync(LinkPreviewRequest url, bool isCircleRedirect = false, bool retryWithoutCompressionOnFailure = true, bool noCompression = false);
    }
}
