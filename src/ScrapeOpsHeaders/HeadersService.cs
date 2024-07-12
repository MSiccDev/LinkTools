using System.Text.Json;

namespace MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

public class HeadersService : IHeadersService
{
    private static readonly string BaseUrl = "https://headers.scrapeops.io/v1/";

    private static HttpClient Client { get; } = new HttpClient();
    

    public async Task<UserAgentsResponse?> GetUserAgents(string apiKey, int count = 10)
    {
        var requestUrl = $"{BaseUrl}user-agents?api_key={apiKey}&num_result={count}";

        var json = await Client.GetStringAsync(requestUrl);

        var result = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<UserAgentsResponse>(json);

        if (result != null)
            result.Raw = json;

        return result;
    }
    
    public async Task<HeadersResponse?> GetBrowserHeaders(string apiKey, int count = 10)
    {
        var requestUrl = $"{BaseUrl}browser-headers?api_key={apiKey}&num_result={count}";
        
        var json = await Client.GetStringAsync(requestUrl);

        var result = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<HeadersResponse>(json);
        
        if (result != null)
            result.Raw = json;

        return result;
    }
}




