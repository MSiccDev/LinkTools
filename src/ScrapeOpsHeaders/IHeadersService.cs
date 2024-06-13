namespace MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

public interface IHeadersService
{
    Task<UserAgentsResponse?> GetUserAgents(string apiKey, int count = 10);
    Task<HeadersResponse?> GetBrowserHeaders(string apiKey, int count = 10);
}