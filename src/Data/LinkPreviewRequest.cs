using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class LinkPreviewRequest
    {
        [JsonConstructor]
        public LinkPreviewRequest()
        {

        }

        public LinkPreviewRequest(Uri? originalUrl)
        {
            this.OriginalUrl = originalUrl;
            this.Result = new LinkInfo(this.OriginalUrl);
            this.CurrentRequestedUrl = this.OriginalUrl;
        }

        public Uri? OriginalUrl { get; set; }

        public Uri? CurrentRequestedUrl { get; set; }

        public HttpResponseMessage? OriginalResponse { get; set; }


        public Dictionary<string, HttpResponseMessage?> Redirects = new Dictionary<string, HttpResponseMessage?>();


        public RequestError? Error { get; set; }

        [JsonIgnore]
        public bool IsSuccess => this.Error == null && this.Result != null;

        public LinkInfo? Result { get; set; }
    }
}
