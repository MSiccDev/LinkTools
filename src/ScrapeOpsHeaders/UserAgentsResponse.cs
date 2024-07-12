using System.Text.Json.Serialization;

namespace MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

public class UserAgentsResponse
{
    [JsonPropertyName("result")]
    public List<string>? Results { get; set; }
    
    [JsonIgnore]
    public string? Raw { get; set; }
}