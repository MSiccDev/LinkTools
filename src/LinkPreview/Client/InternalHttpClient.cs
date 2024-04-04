using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
    public class InternalHttpClient
    {
        private HttpClient _httpClientInstance;
        internal InternalHttpClient()
        {

        }

        public HttpClient GetStaticClient(HttpClientConfiguration configuration)
        {

            if (_httpClientInstance == null)
            {
                var handler = new SocketsHttpHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    AllowAutoRedirect = configuration.AllowRedirect,
                    UseCookies = configuration.UseCookies
                };

                _httpClientInstance = new HttpClient(handler);

                if (configuration.ModifiedSince.HasValue)
                {
                    _httpClientInstance.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(configuration.ModifiedSince.Value);
                }

                if (configuration.UseCompression)
                {
                    _httpClientInstance.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                    _httpClientInstance.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
                }

                _httpClientInstance.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(configuration.AcceptHeader));
                _httpClientInstance.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(configuration.AcceptCharsetHeader));
                _httpClientInstance.Timeout = configuration.Timeout;


                if (string.IsNullOrEmpty(configuration.CustomUserAgentString))
                {
                    var assembly = this.GetType().Assembly;

                    _httpClientInstance.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(string.IsNullOrEmpty(configuration.DefaultUserAgent) ? assembly.GetName().Name : configuration.DefaultUserAgent,
                                                   string.IsNullOrEmpty(configuration.UserAgentVersion) ? assembly.GetName().Version.ToString() : configuration.UserAgentVersion));
                }
                else
                    _httpClientInstance.DefaultRequestHeaders.Add("user-agent", configuration.CustomUserAgentString);

            }

            return _httpClientInstance;
        }

        public HttpClient GetTemporaryClient(HttpClientConfiguration configuration)
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = configuration.AllowRedirect,
                UseCookies = configuration.UseCookies
            };

            var client = new HttpClient(handler);

            if (configuration.ModifiedSince.HasValue)
            {
                client.DefaultRequestHeaders.IfModifiedSince = new DateTimeOffset(configuration.ModifiedSince.Value);
            }

            if (configuration.UseCompression)
            {
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(configuration.AcceptHeader));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue(configuration.AcceptCharsetHeader));
            client.Timeout = configuration.Timeout;

            if (string.IsNullOrEmpty(configuration.CustomUserAgentString))
            {
                var assembly = this.GetType().Assembly;

                client.DefaultRequestHeaders.UserAgent.Add(
                    new ProductInfoHeaderValue(string.IsNullOrEmpty(configuration.DefaultUserAgent) ? assembly.GetName().Name : configuration.DefaultUserAgent,
                                               string.IsNullOrEmpty(configuration.UserAgentVersion) ? assembly.GetName().Version.ToString() : configuration.UserAgentVersion));
            }
            else
                client.DefaultRequestHeaders.Add("user-agent", configuration.CustomUserAgentString);

            return client;
        }
    }
}
