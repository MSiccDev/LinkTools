using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using MSiccDev.Libs.LinkTools;
using MSiccDev.Libs.LinkTools.LinkPreview;
using MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;
using Newtonsoft.Json;

namespace LinkToolsTestConsole
{
	class Program
	{
		private static LinkPreviewService _linkPreviewService;

		static async Task Main(string[] args)
		{
			Console.WriteLine("Hello World!");

			//await TestScrapeOpsHeaders();

			await UrlCleaner.Current.InitializeAsync();
			
			_linkPreviewService = new LinkPreviewService();
			
			await TestGetAllLinksParallelAsync();



			//var cleanedUrlRaiNews1 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.fb.red_scuola.&wt"));
			//var cleanedUrlRaiNews2 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.tw.red_scuola.&wt"));


			//await TestCleanUrlsFromFileAsync();



			Console.ReadLine();
		}

		public static async Task<List<Uri>> GetUrisFromJsonAsync()
		{
			var assembly = typeof(Program).Assembly;

			var resourceFile = "LinkToolsTestConsole.LinksToTest.json";

			using var resourceStream = assembly.GetManifestResourceStream(resourceFile);
			using var streamReader = new StreamReader(resourceStream);

			var fileContent = await streamReader.ReadToEndAsync();

			if (!string.IsNullOrEmpty(fileContent))
				return JsonConvert.DeserializeObject<List<Uri>>(fileContent);

			return null;
		}

		public static async Task<List<LinkPreviewRequest>> GetLinkPreviewRequestsAsync()
		{
			var urls = await GetUrisFromJsonAsync();

			Console.WriteLine($"Loaded {urls.Count} urls from json file");

			return urls.Select(u => new LinkPreviewRequest(UrlCleaner.Current.CleanUrl(u))).ToList();
		}

		public static async Task TestGetAllLinksParallelAsync()
		{
			var requests = await GetLinkPreviewRequestsAsync();

			Console.WriteLine($"got {requests.Count} link preview requests");

			var tasks = requests.Select(r => _linkPreviewService.GetLinkDataAsync(r, includeDescription: true)).ToList();

			Console.WriteLine($"running all requests");
			var results = await Task.WhenAll(tasks).ConfigureAwait(false);

			foreach (var result in results.Where(r => r.Error != null))
			{
				Console.WriteLine($"Error requesting link {result.OriginalUrl}");
			}

			foreach (var result in results.Where(r => r.Error == null))
			{
				if (result.Result != null)
					Console.WriteLine($"{result.OriginalUrl}: Title - {result.Result.Title}; ImageUrl = {result.Result.ImageUrl}");
			}
			
			Console.WriteLine($"finsihed loading {requests.Count} requests");
		}

		public static async Task TestCleanUrlsFromFileAsync()
		{
			var urlsFromFile = await GetUrisFromJsonAsync();

			foreach (var url in urlsFromFile)
			{
				var cleanedUrl = UrlCleaner.Current.CleanUrl(url);

				Console.WriteLine($"Original: {url}");
				Console.WriteLine($"Cleaned: {cleanedUrl}");
			}
		}


		public static async Task TestScrapeOpsHeaders()
		{
			var apiClientKey = "{get your own key!}";
			apiClientKey = "0b68441a-f6f7-4159-9740-cbeaf04bcdf6";

			var service = new HeadersService();

			var useragents = await service.GetUserAgents(apiClientKey);

			if (useragents != null)
			{
				Console.WriteLine($"Got {useragents.Results?.Count} results:");
				foreach (var result in useragents.Results)
					Console.WriteLine(result);
			}
			else
			{
				Console.WriteLine("No results, something must have gone wrong.");
			}

			var headers = await service.GetBrowserHeaders(apiClientKey);

			if (headers != null)
			{
				Console.WriteLine($"Got {headers.Results?.Count} results:");
				foreach (var result in headers.Results)
				{
					Console.WriteLine("HeadersCollection:");
					foreach (var pair in result)
					{
						Console.WriteLine($"{pair.Key},{pair.Value}");
					}
				}
			}
			else
			{
				Console.WriteLine("No results, something must have gone wrong.");
			}
			
			
		}
		
		
	}
}
