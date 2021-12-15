using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools
{
    public class UrlCleaner
    {
        private static UrlCleaner _instance;

        private CampaignParams _campaignParams;

        public static UrlCleaner Current => _instance ??= new UrlCleaner();

        public UrlCleaner()
        {

        }

        public async Task InitializeAsync()
        {
            _campaignParams = await LoadDefaultParamsFromJsonFile();

            IsInitialized = true;
        }

        private async Task<CampaignParams> LoadDefaultParamsFromJsonFile()
        {
            CampaignParams campaigns = null;

            var assembly = typeof(UrlCleaner).Assembly;

            var resourceFile = "MSiccDev.Libs.LinkTools.campaignparams.json";

            using var resourceStream = assembly.GetManifestResourceStream(resourceFile);
            using var streamReader = new StreamReader(resourceStream);

            var fileContent = await streamReader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(fileContent))
                campaigns = JsonConvert.DeserializeObject<CampaignParams>(fileContent);

            return campaigns;
        }


        private List<string> GetQueryParameters(Uri url)
        {
            List<string> result = new List<string>();

            if (!string.IsNullOrWhiteSpace(url.Query))
            {
                //remove '?'
                var paramString = url.Query.Substring(1);

                ParseParameters(result, paramString);
            }

            return result;
        }
        private List<string> GetFragmentParameters(Uri url)
        {
            List<string> result = new List<string>();

            if (!string.IsNullOrWhiteSpace(url.Fragment))
            {
                //remove '#'
                var paramString = url.Fragment.Substring(1);

                ParseParameters(result, paramString);
            }

            return result;
        }


        private static void ParseParameters(List<string> result, in string paramString)
        {
            var parameters = paramString.Split('&');

            if (parameters?.Any() ?? false)
            {
                foreach (var param in parameters)
                    result.Add(param);
            }
        }


        public Uri CleanUrl(Uri url)
        {
            Uri result = url;
            var urlString = result.ToString();

            //var regexPattern = @"([_a-zA-Z0-9=]+)";

            var queryParams = GetQueryParameters(result);
            var fragments = GetFragmentParameters(result);

            if (queryParams.Any())
            {
                queryParams.Reverse();

                foreach (var param in queryParams)
                {
                    urlString = RemoveParamFromUrlString(urlString, param);
                }

            }
            
            if (fragments.Any())
            {
                fragments.Reverse();

                foreach (var param in fragments)
                {
                    urlString = RemoveParamFromUrlString(urlString, param);
                }
            }

            return new Uri(urlString);



            string RemoveParamFromUrlString(string urlString, string param)
            {
                foreach (var campaign in _campaignParams.Campaigns)
                {
                    foreach (var matchingParam in campaign.Parameters.Where(defaultParam => param.StartsWith(defaultParam)))
                    {
                        if (campaign.IsDomainExclusive &&
                            !url.Authority.Contains(campaign.Name.ToLowerInvariant().Replace(".*", string.Empty)))
                            return urlString;

                        //-1 one because we removed '&' and '?' already from param
                        var indexofParam = urlString.IndexOf(param) - 1;
                        //removing the last param if is a match

                        if (indexofParam < 0)
                            continue;
                        else
                            urlString = urlString.Remove(indexofParam);
                    }                
                }
                return urlString;
            }

        }


        public bool IsInitialized { get; private set; }

    }
}
