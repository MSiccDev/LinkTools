using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class HttpClientConfiguration
    {
        public DateTime? ModifiedSince { get; set; } = null;

        public string DefaultUserAgent { get; set; } = null;

        public string UserAgentVersion { get; set; } = null;

        public string AcceptHeader { get; set; } = "application/json";

        public string AcceptCharsetHeader { get; set; } = "UTF-8";

        public string CustomUserAgentString { get; set; } = null;

        public bool AllowRedirect { get; set; } = true;

        public bool UseCookies { get; set; } = true;

        public bool UseCompression { get; set; } = true;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }

    public class DefaultHttpClientConfiguration : HttpClientConfiguration
    {

    }

    public class NoCompressionHttpClientConfiguration : HttpClientConfiguration
    {
        public NoCompressionHttpClientConfiguration()
        {
            this.UseCompression = false;
        }
    }
}
