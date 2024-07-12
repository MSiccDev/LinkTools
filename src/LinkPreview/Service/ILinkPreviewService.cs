using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public interface ILinkPreviewService
	{
		Task<LinkPreviewRequest> GetLinkDataAsync(LinkPreviewRequest url, bool isCircleRedirect = false,
			bool retryWithoutCompressionOnFailure = true, bool noCompression = false,
			bool addCookieToRedirectedRequest = false, bool includeDescription = false, bool useScrapeOpsHeaders = false);


		Task<HeadersResponse?> RefreshScrapeOpsHeadersAsync(string apiKey);
		Dictionary<string, string>? GetRandomScrapeOpsHeaders();
		void SetCurrentHeadersCollection(HeadersResponse headersResponse);
	}
}
