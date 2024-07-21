using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using MSiccDev.Libs.LinkTools;
using MSiccDev.Libs.LinkTools.LinkPreview;
using MSiccDev.Libs.LinkTools.ScrapeOpsHeaders;
using Newtonsoft.Json;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

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

			var headerService = new HeadersService();
			
			_linkPreviewService = new LinkPreviewService(headerService);
			
			//await TestGetAllLinksParallelAsync();

			//await TestGetAllLinksFromFeedItemModelAsync();



			//var cleanedUrlRaiNews1 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.fb.red_scuola.&wt"));
			//var cleanedUrlRaiNews2 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.tw.red_scuola.&wt"));


			// var zdImageUri =
			// 	new Uri(
			// 		"https://www.zdnet.com/a/img/resize/c37ec47e740fd97f8f05d7977b11a639ede3d0e8/2023/08/22/a4729d1c-9bc0-42ce-836b-8f9d459dcb01/best-foldable-phones-zdnet-thumb-image.jpg?auto=webp&amp;fit=crop&amp;height=675&amp;width=1200");
			// var cleanedUrlZdNet = UrlCleaner.Current.CleanUrl(zdImageUri);

			//await TestCleanUrlsFromFileAsync();
			
			Console.ReadLine();
		}

		public static async Task<List<Uri>?> GetUrisFromJsonAsync()
		{
			var assembly = typeof(Program).Assembly;

			var resourceFile = "LinkToolsTestConsole.LinksToTest.json";

			using var resourceStream = assembly.GetManifestResourceStream(resourceFile);
			using var streamReader = new StreamReader(resourceStream);

			var fileContent = await streamReader.ReadToEndAsync();

			return !string.IsNullOrEmpty(fileContent) ? JsonConvert.DeserializeObject<List<Uri>>(fileContent) : null;
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

			var tasks = requests.Select(r => _linkPreviewService.GetLinkDataAsync(r, includeDescription: true, useScrapeOpsHeaders: true)).ToList();

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
		
		public static async Task TestGetAllLinksFromFeedItemModelAsync()
		{
			Console.WriteLine("Enter the full path to the exported file from TwistReader:");
			var fileName = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(fileName))
			{
				Console.WriteLine("no file name entered, restart to try again");
				return;
			}
			
			var json = await File.ReadAllTextAsync(fileName);

			var feedItemModels = JsonConvert.DeserializeObject<List<FeedItemModelWithLinkPreviewResponse>>(json);

			if (feedItemModels is null)
			{
				Console.WriteLine("No FeedItemModel found, restart to try again");
				return;
			}

			await _linkPreviewService.RefreshScrapeOpsHeadersAsync("956157ed-aa0a-48e1-af57-465666ffd1f2").ConfigureAwait(false);

			var linkPreviewRequests = new List<Task<LinkPreviewRequest>>();
			foreach (var model in feedItemModels)
			{
				model.LinkPreviewRequest = new LinkPreviewRequest(model.Link);
				
				linkPreviewRequests.Add(_linkPreviewService.GetLinkDataAsync(model.LinkPreviewRequest, useScrapeOpsHeaders: true));
			}

			Console.WriteLine($@"Trying to fetch {linkPreviewRequests.Count} link previews...");
			
			while (linkPreviewRequests.Count > 0)
			{
				try
				{
					var firstFinished = await Task.WhenAny(linkPreviewRequests).ConfigureAwait(false);
                    
					linkPreviewRequests.Remove(firstFinished);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}  
			}
			
			Console.WriteLine("All link preview requests done, do you want to save the results? (type Y to save)");
			var wantsToSave = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(wantsToSave) || wantsToSave.ToUpper() != "Y")
			{
				Console.WriteLine("Stopping...");
				return;
			}



			var jsonToSave = JsonConvert.SerializeObject(feedItemModels, Formatting.Indented);

			var newFileName = fileName.Replace(".json", "_with_link_fetching_results.json");
			await File.WriteAllTextAsync(newFileName, jsonToSave);
			
			Console.WriteLine("File saved, stopping now.");
		}
		
	}
}
