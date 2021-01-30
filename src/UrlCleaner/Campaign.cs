using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools
{
    public class Campaign
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("params")]
        public List<string> Parameters { get; set; }

        [JsonProperty("docs", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Docs { get; set; }

        [JsonIgnore]
        public bool IsDomainExclusive => !string.IsNullOrEmpty(this.DomainName);

        [JsonProperty("exlusivedomain", NullValueHandling = NullValueHandling.Include)]
        public string DomainName { get; set; }
    }

    public class CampaignParams
    {
        [JsonProperty("Campaigns")]
        public List<Campaign> Campaigns { get; set; }
    }
}
