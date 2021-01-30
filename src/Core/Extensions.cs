using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSiccDev.Libs.LinkTools
{
    public static partial class Extensions
    {
        public static bool IsHttps(this string url)
        {
            var parser = new Regex(@"\b(?:https://)\S+\b", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

            return parser.IsMatch(url);
        }

        public static string EnforceHttps(this string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;
            try
            {
                var parser = new Regex(@"\b(?:http?://)\S+\b", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

                if (parser.IsMatch(url))
                {
                    url = Regex.Replace(url, "http", "https");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}:{ex.Message} for {url} in {nameof(EnforceHttps)}");
            }

            return url;
        }

        public static string ToArrayString(this int[] array)
        {
            if (array.Length == 1)
                return array[0].ToString();

            if (array.Length > 1)
            {
                var sb = new StringBuilder();

                array.ToList().ForEach(i => sb
                    .Append(i)
                    .Append(","));

                var result = sb.ToString();

                return result.EndsWith(",") ? result.Substring(0, result.Length - 1) : result;
            }

            return string.Empty;
        }




        public static string RemoveUrlParameters(this string url)
        {
            if (url.Contains("youtube.com/watch"))
                return url;

            if (url.Contains("?"))
                return url.Substring(0, url.IndexOf("?"));

            if (url.Contains("#"))
                url = url.Substring(0, url.IndexOf("#"));

            return url;
        }

        public static bool ContainsParameter(this Uri url, string key)
        {
            var urlString = Uri.UnescapeDataString(url.ToString());

            if (!urlString.Contains('?'))
                return false;

            var parameters = System.Web.HttpUtility.ParseQueryString(urlString.Substring(urlString.IndexOf('?')));

            return parameters.AllKeys.Contains(key);
        }

        public static string AddParameterToUrl(this string url, string parameterName, string parameterValue)
        {
            if (url.Contains("?"))
            {
                return $"{url}&{parameterName}={parameterValue}";
            }
            else
            {
                return $"{url}?{parameterName}={parameterValue}";
            }
        }

        public static string AddParametersToUrl(this string url, Dictionary<string, string> parameters)
        {
            var result = url;

            if (parameters.Count > 0)
            {
                foreach (var p in parameters)
                {
                    result = result.AddParameterToUrl(p.Key, p.Value);
                }
            }

            return result;
        }




    }
}
