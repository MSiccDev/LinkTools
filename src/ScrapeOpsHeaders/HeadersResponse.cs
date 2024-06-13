using System.Text.Json.Serialization;

namespace MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

public class HeadersResponse
{
    [JsonPropertyName("result")]
    public List<Dictionary<string, string>>? Results { get; set; }
}