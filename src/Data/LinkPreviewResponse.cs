using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class LinkPreviewResponse
    {
        [JsonConstructor]
        public LinkPreviewResponse()
        {

        }

        public LinkPreviewResponse(LinkPreviewRequest request)
        {
            this.Error = request.Error;
            this.Result = request.Result;

            this.Redirects = new List<RedirectInfo>();

            if (request.Redirects?.Any() ?? false)
            {
                var urls = request.Redirects.Keys.ToList();

                for (int i = 0; i < urls.Count; i++)
                {
                    if (i == 0)
                    {
                        this.Redirects.Add(new RedirectInfo()
                        {
                            RequestedUrl = request.OriginalUrl,
                            RedirectUrl = new Uri(urls[i])
                        });
                    }
                    else if (i > 0)
                    {
                        this.Redirects.Add(new RedirectInfo()
                        {
                            RequestedUrl = new Uri(urls[i - 1]),
                            RedirectUrl = new Uri(urls[i])
                        });
                    }
                }
            }
        }

        public RequestError Error { get; set; }

        [JsonIgnore]
        public bool IsSuccess => this.Error == null && this.Result != null;


        public LinkInfo Result { get; set; }

        public List<RedirectInfo> Redirects { get; set; }
    }
}
