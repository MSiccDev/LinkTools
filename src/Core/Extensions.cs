using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public static bool ContainsParameter(this Uri? url, string key)
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


        public static string RecursiveHtmlDecode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var tmp = WebUtility.HtmlDecode(text);
            while (tmp != text)
            {
                text = tmp;
                tmp = WebUtility.HtmlDecode(text);
            }
            return text;
        }

        public static string RecursiveUrlDecode(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            var tmp = WebUtility.UrlDecode(text);
            while (tmp != text)
            {
                text = tmp;
                tmp = WebUtility.UrlDecode(text);
            }
            return text;
        }


        internal const string LinkPattern = @"(?i)\b((?:https?:(?:/{1,3}|[a-z0-9%])|[a-z0-9.\-]+[.](?:com|net|org|edu|gov|mil|aero|asia|biz|cat|coop|info|int|jobs|mobi|museum|name|post|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|Ja|sk|sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw)/)(?:[^\s()<>{}\[\]]+|\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\))+(?:\([^\s()]*?\([^\s()]+\)[^\s()]*?\)|\([^\s]+?\)|[^\s`!()\[\]{};:'.,<>?«»“”‘’])|(?:(?<!@)[a-z0-9]+(?:[.\-][a-z0-9]+)*[.](?:com|net|org|edu|gov|mil|aero|asia|biz|cat|coop|info|int|jobs|mobi|museum|name|post|pro|tel|travel|xxx|ac|ad|ae|af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|ax|az|ba|bb|bd|be|bf|bg|bh|bi|bj|bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|cs|cu|cv|cx|cy|cz|dd|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|eu|fi|fj|fk|fm|fo|fr|ga|gb|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|me|mg|mh|mk|ml|mm|mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|rs|ru|rw|sa|sb|sc|sd|se|sg|sh|si|sj|Ja|sk|sl|sm|sn|so|sr|ss|st|su|sv|sx|sy|sz|tc|td|tf|tg|th|tj|tk|tl|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|us|uy|uz|va|vc|ve|vg|vi|vn|vu|wf|ws|ye|yt|yu|za|zm|zw)\b/?(?!@)))";

        public static string[] ToPlainUrls(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var linksInText = Regex.Matches(text, LinkPattern, RegexOptions.Multiline);

            var links = new List<string>();

            foreach (var link in linksInText.Cast<object>().Where(link => !links.Contains(link.ToString())))
            {
                links.Add(link.ToString());
            }

            return links.ToArray();
        }

        public static string ToPlainTextWithoutUrls(this string text, bool removeOnlyFirst = false)
        {
            string cleanedText = "";

            var urls = text.ToPlainUrls();

            if (urls?.Length > 0)
            {
                if (removeOnlyFirst)
                {
                    var removedLinkText = text.Replace(urls.FirstOrDefault(), "");
                    cleanedText = removedLinkText.Replace("  ", " ");
                }
                else
                {
                    foreach (var u in urls)
                    {
                        if (text.Contains(u))
                        {
                            var removedLinkText = text.Replace(u, "");
                            cleanedText = removedLinkText.Replace("  ", " ");
                        }
                    }
                }
            }
            else
            {
                cleanedText = text;
            }
            return cleanedText;
        }

    }
}
