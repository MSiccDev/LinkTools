//using HttpCompletionOption.ResponseHeadersRead solves quite some problems,
//it all started with nzz.ch, luckily we solved it following this post:
//https://stackoverflow.com/questions/33233780/system-net-http-httprequestexception-error-while-copying-content-to-a-stream
//as I am already checking for the status codes of the response,
//this was a no brainer to add

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class LinkPreviewService : ILinkPreviewService
	{
		private readonly HttpClient _client;

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IHeadersService _headersService;
		
		private List<Dictionary<string,string>>? _scrapeOpsHeadersCollection;

		public LinkPreviewService(IHttpClientFactory httpClientFactory, IHeadersService headersService)
		{
			_httpClientFactory = httpClientFactory;
			_headersService = headersService;
			
			_client = httpClientFactory.CreateClient(nameof(LinkPreviewService));
		}

		public async Task<HeadersResponse?> RefreshScrapeOpsHeadersAsync(string apiKey)
		{
			var latestHeaders = await _headersService.GetBrowserHeaders(apiKey);

			if (latestHeaders == null)
				return null;
			
			_scrapeOpsHeadersCollection = latestHeaders.Results;

			return latestHeaders;
		}

		public void SetCurrentHeadersCollection(HeadersResponse headersResponse) => 
			_scrapeOpsHeadersCollection = headersResponse.Results;

		public Dictionary<string, string>? GetRandomScrapeOpsHeaders()
		{
			if (_scrapeOpsHeadersCollection is null or { Count: 0 })
				return null;
            
			var random = new Random();
			var randomIndex = random.Next(0, _scrapeOpsHeadersCollection.Count - 1);

			return _scrapeOpsHeadersCollection[randomIndex];
		}
		
		

		public async Task<LinkPreviewRequest> GetLinkDataAsync(
		    LinkPreviewRequest previewRequest,
		    bool isCircleRedirect = false,
		    bool addCookieToRedirectedRequest = false,
		    bool includeDescription = false,
		    bool useScrapeOpsHeaders = false,
		    CancellationToken cancellation = default)
		{
		    try
		    {
		        // Special‑case: Facebook exit links
		        if (previewRequest.CurrentRequestedUrl.ToString().Contains("facebook.com") &&
		            previewRequest.CurrentRequestedUrl.ContainsParameter("u"))
		        {
		            return await HandleFacebookExitLink(previewRequest, cancellation);
		        }

		        // Delegate the heavy lifting
		        return await ProcessRequestAsync(
		            previewRequest,
		            isCircleRedirect,
		            addCookieToRedirectedRequest,
		            includeDescription,
		            useScrapeOpsHeaders,
		            cancellation);
		    }
		    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
		    {
			    throw;
		    }
		    catch (Exception ex)
		    {
			    previewRequest.Error = new RequestError(ex);
			    Console.WriteLine($"{ex.GetType()}: {ex.Message} for url {previewRequest.CurrentRequestedUrl} in {nameof(LinkPreviewService)}");
			    return previewRequest;
		    }
		}


		private void ConfigureRequestHeaders(HttpRequestMessage request)
		{
			//parse headers and add them to the request
			var randomHeaders = GetRandomScrapeOpsHeaders();
			
			if (randomHeaders == null)
				return;
			
			request.Headers.Clear();

			foreach (var header in randomHeaders)
			{
				request.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
		}

		private HttpRequestMessage BuildHttpRequest(Uri uri, string? cookieHeaderValue, bool useScrapeOpsHeaders)
		{
		    var request = new HttpRequestMessage(uri.ToString().IsHttps() ? HttpMethod.Get : HttpMethod.Head, uri);

		    if (useScrapeOpsHeaders)
		        ConfigureRequestHeaders(request);

		    request.Headers.Host = uri.Host;

		    if (!string.IsNullOrWhiteSpace(cookieHeaderValue))
		        request.Headers.Add("Cookie", cookieHeaderValue);

		    return request;
		}

		private async Task<LinkPreviewRequest> ProcessRequestAsync(
		    LinkPreviewRequest previewRequest,
		    bool isCircleRedirect,
		    bool addCookieToRedirectedRequest,
		    bool includeDescription,
		    bool useScrapeOpsHeaders,
		    CancellationToken cancellation)
		{
		    if (!isCircleRedirect)
		    {
		        string? cookieHeaderValue = null;
		        if (addCookieToRedirectedRequest)
		            cookieHeaderValue = TryExtractCookieValueFromLastResponse(previewRequest);

		        var request = BuildHttpRequest(previewRequest.CurrentRequestedUrl, cookieHeaderValue, useScrapeOpsHeaders);

		        previewRequest.UsedHeaders = request.Headers.ToDictionary();

		        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation);

		        if (previewRequest.OriginalResponse == null)
		            previewRequest.OriginalResponse = response;
		        else
		            previewRequest.Redirects.Add(previewRequest.CurrentRequestedUrl.ToString(), response);

		        var statusCode = (int)response.StatusCode;

		        if (statusCode >= 300 && statusCode <= 399)
		        {
		            return await HandleRedirect(response, previewRequest, cancellation);
		        }
		        else if (statusCode >= 400)
		        {
		            var message = await response.Content.ReadAsStringAsync(cancellation).ConfigureAwait(false);
		            previewRequest.Error = new RequestError(statusCode, message);
		            Console.WriteLine($"got error response ({statusCode}) from {previewRequest.CurrentRequestedUrl}\nmessage: {message}");
		        }
		        else
		        {
		            var linkPreview = await TryGetLinkPreview(response, includeDescription, cancellation);
		            previewRequest.Result = linkPreview;
		        }
		    }
		    else
		    {
		        await TryGetLinkDataFrom302Redirects(previewRequest, includeDescription, cancellation).ConfigureAwait(false);
		    }

		    return previewRequest;
		}


		private async Task<LinkPreviewRequest> HandleFacebookExitLink(LinkPreviewRequest previewRequest, CancellationToken cancellation)
		{
			var correctLink = previewRequest.CurrentRequestedUrl.TryGetLinkFromFacebookExitLink();

			if (correctLink != null)
				previewRequest.CurrentRequestedUrl = correctLink;

			return await GetLinkDataAsync(previewRequest, false, cancellation: cancellation);
		}


		private async Task TryGetLinkDataFrom302Redirects(LinkPreviewRequest previewRequest, bool includeDescription, CancellationToken cancellation)
		{
			if (previewRequest.OriginalResponse.StatusCode == HttpStatusCode.Found)
			{
				var linkPreview = await TryGetLinkPreview(previewRequest.OriginalResponse, includeDescription, cancellation);
				previewRequest.Result = linkPreview;
			}
			else if (previewRequest.Redirects.Values.Any(r => r.StatusCode == HttpStatusCode.Found))
			{
				var linkPreviewTasks = new List<Task<LinkInfo>>();
				foreach (var response in previewRequest.Redirects.Values.Where(r => r.StatusCode == HttpStatusCode.Found))
				{
					linkPreviewTasks.Add(TryGetLinkPreview(response, includeDescription, cancellation));
				}

				var linkPreviews = await Task.WhenAll(linkPreviewTasks).ConfigureAwait(false);

				var linkWithTitleAndImage = linkPreviews.FirstOrDefault(p => !string.IsNullOrEmpty(p.Title) && p.ImageUrl != null);
				previewRequest.Result = linkWithTitleAndImage ?? linkPreviews.FirstOrDefault(p => !string.IsNullOrEmpty(p.Title));
			}
		}


		private async Task<LinkPreviewRequest> HandleRedirect(HttpResponseMessage? response, LinkPreviewRequest previewRequest, CancellationToken cancellation)
		{
			var redirectUri = response.Headers.Location;

			if (redirectUri != null)
			{
				Console.WriteLine($"got redirect ({response.StatusCode}) from {previewRequest.CurrentRequestedUrl} to {redirectUri} (https: {redirectUri.ToString().IsHttps()})");

				if (redirectUri.ToString() == previewRequest.CurrentRequestedUrl.ToString())
				{
					if (!response.Headers.Any(header => header.Key == "Set-Cookie"))
						return await GetLinkDataAsync(previewRequest, true, cancellation: cancellation);
					else
						return await GetLinkDataAsync(previewRequest, false, true, cancellation: cancellation);
				}

				var redirectUriString = redirectUri.ToString();
				if (!redirectUriString.IsHttps())
				{
					//supporting also relative urls
					if (!redirectUri.IsAbsoluteUri)
					{
						if (redirectUriString.StartsWith("//"))
							redirectUriString = $"{response.RequestMessage.RequestUri.Scheme}:{redirectUriString}";
						else
							redirectUriString = $"{response.RequestMessage.RequestUri.GetLeftPart(UriPartial.Authority)}{redirectUriString}";
					}
				}

				if (!previewRequest.Redirects.ContainsKey(redirectUriString))
				{
					previewRequest.CurrentRequestedUrl = new Uri(redirectUriString);

					return await GetLinkDataAsync(previewRequest, cancellation: cancellation);
				}
				else
				{
					return await GetLinkDataAsync(previewRequest, true, cancellation: cancellation);
				}
			}

			Console.WriteLine($"got redirect ({response.StatusCode}) from {previewRequest.CurrentRequestedUrl} with no location header)");

			return null;
		}
		

		private async Task<LinkInfo> TryGetLinkPreview(HttpResponseMessage? response, bool includeDescription, CancellationToken cancellation)
		{
			var responseContentStream = await response.Content.ReadAsStreamAsync(cancellation);

			var streamReader = new StreamReader(responseContentStream, Encoding.UTF8);
			var html = await streamReader.ReadToEndAsync();

			html = Regex.Replace(html, @"\t|\n|\r", "");

			return !string.IsNullOrWhiteSpace(html) ? html.ToLinkInfo(response.RequestMessage.RequestUri, includeDescription) : null;
		}

		private string TryExtractCookieValueFromLastResponse(LinkPreviewRequest previewRequest)
		{
			HttpResponseMessage? cookieContainingResponse = null;
			var lastResponse = previewRequest.Redirects.LastOrDefault();

			cookieContainingResponse = lastResponse.Value == null ? previewRequest.OriginalResponse : lastResponse.Value;

			var cookies = cookieContainingResponse.Headers.SingleOrDefault(header => header.Key == "Set-Cookie");

			var cookieValues = cookies.Value.Select(cookie => cookie.Substring(0, cookie.IndexOf(";") + 1)).ToList();

			var cookieValueStringBuilder = new StringBuilder();

			foreach (var value in cookieValues)
				cookieValueStringBuilder.Append($"{value} ");

			return cookieValueStringBuilder.ToString().Trim();
		}
	}
}
