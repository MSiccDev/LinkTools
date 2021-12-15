using System;
using System.Threading.Tasks;

using MSiccDev.Libs.LinkTools;

namespace LinkToolsTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            await TestCleanUrl("https://www.wired.com/2014/10/astrophysics-interstellar-black-hole/?mbid=social_twitter&utm_source=twitter&utm_brand=wired&utm_medium=social&utm_social-type=owned&utm_brand=wired&utm_source=twitter&utm_medium=social&utm_social-type=owned&mbid=social_twitter");
            await TestCleanUrl("https://edition.cnn.com/videos/us/2021/12/10/new-york-city-holiday-lights-return-2021-field-dnt-vpx.cnn?utm_source=twCNNi&utm_term=video&utm_content=2021-12-14T03:34:42&utm_medium=social");
        }

        public static async Task TestCleanUrl(string url)
        {
            if (!UrlCleaner.Current.IsInitialized)
                await UrlCleaner.Current.InitializeAsync();

            Console.WriteLine("Trying to clean url...");
            Console.WriteLine($"original url {url}");

            try
            {
                var urlUri = new Uri(url);
                var cleanedUrl = UrlCleaner.Current.CleanUrl(urlUri);

                Console.WriteLine($"cleaned url {cleanedUrl}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
