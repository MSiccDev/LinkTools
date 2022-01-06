using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using MSiccDev.Libs.LinkTools;
using MSiccDev.Libs.LinkTools.LinkPreview;

using Newtonsoft.Json;

namespace LinkToolsTestConsole
{
    class Program
    {
        private static LinkPreviewService _linkPreviewService;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await UrlCleaner.Current.InitializeAsync();

            _linkPreviewService = new LinkPreviewService();

            //await TestGetAllLinksParallelAsync();



            //var cleanedUrlRaiNews1 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.fb.red_scuola.&wt"));
            //var cleanedUrlRaiNews2 = UrlCleaner.Current.CleanUrl(new Uri("https://www.rai24.it/articoli/2022/01/scuola-bfdcb38b-2dd8-44b0-812f-72e57fca9a8c.html?wt_mc=2.social.tw.red_scuola.&wt"));


            await TestCleanUrlsFromFileAsync();



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

            var tasks = requests.Select(r => _linkPreviewService.GetLinkDataAsync(r)).ToList();

            Console.WriteLine($"running all requests");
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var result in results.Where(r => r.Error != null))
            {
                Console.WriteLine($"Error requesting link {result.OriginalUrl}");
            }

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


    }
}
