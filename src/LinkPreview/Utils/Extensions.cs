using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using MSiccDev.Libs.LinkTools;
using MSiccDev.Libs.LinkTools.LinkPreview;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MSiccDev.Libs.LinkTools
{
	public static partial class Extensions
	{

		public static Uri? TryGetLinkFromFacebookExitLink(this Uri? url)
		{
			var urlString = Uri.UnescapeDataString(url.ToString());

			if (urlString.Contains("facebook.com"))
			{
				var parameters = System.Web.HttpUtility.ParseQueryString(urlString.Substring(urlString.IndexOf('?')));

				if (parameters != null)
				{
					if (parameters.AllKeys.Contains("u"))
					{
						var urlFromQuery = parameters.Get("u");
						if (!string.IsNullOrWhiteSpace(urlFromQuery))
							return new Uri(urlFromQuery);
					}
				}
			}

			return url;
		}

		public static LinkInfo ToLinkInfo(this string html, Uri? url, bool includeDescription = false)
		{
			if (string.IsNullOrWhiteSpace(html))
				throw new ArgumentException("html must not be null, empty or white space", nameof(html));

			if (url == null)
				throw new ArgumentException("url must not be null", nameof(url));

			var document = new HtmlDocument();
			document.LoadHtml(html);

			//first, read og meta tags
			//second, try to fill missing data from html tags
			//third, try to get canonical url
			var result = new LinkInfo(url)
							.TryParseSchemaOrgFromSelf(document, includeDescription)
							.TryParseOpenGraphMeta(document, includeDescription)
							.TryParseHtmlTagsFromSelf(document, includeDescription)
							.TryGetCanonicalUrl(document);
			return result;
		}

		public static LinkInfo TryParseOpenGraphMeta(this LinkInfo result, HtmlDocument document, bool includeDescription)
		{
			var metaTags = document.DocumentNode.SelectNodes("//meta");

			if (metaTags != null)
			{
				foreach (var tag in metaTags)
				{
					var tagName = tag.Attributes["name"];
					var tagContent = tag.Attributes["content"];
					var tagProperty = tag.Attributes["property"];

					if (tagName != null && tagContent != null)
					{
						switch (tagName.Value.ToLower())
						{
							case "title":
							case "twitter:title":
								result.Title = string.IsNullOrWhiteSpace(result.Title) ? tagContent.Value : result.Title;
								break;
							case "description":
							case "twitter:description":
								if (includeDescription)
									result.Description = string.IsNullOrWhiteSpace(result.Description) ? tagContent.Value : result.Description;
								break;
							case "twitter:image":
								result.ImageUrl ??= (!string.IsNullOrWhiteSpace(tagContent.Value) ? new Uri(tagContent.Value) : null);
								break;
						}
					}
					else if (tagProperty != null && tagContent != null)
					{
						switch (tagProperty.Value.ToLower())
						{
							case "og:title":
								result.Title = string.IsNullOrWhiteSpace(result.Title) ? tagContent.Value : result.Title;
								break;
							case "og:description":
								if (includeDescription)
									result.Description = string.IsNullOrWhiteSpace(result.Description) ? tagContent.Value : result.Description;
								break;
							case "og:image":
								result.ImageUrl = result.ImageUrl == null ? new Uri(tagContent.Value.EnforceHttps()) : new Uri(result.ImageUrl.ToString().EnforceHttps());
								break;
						}
					}
				}
			}

			return result;
		}

		public static LinkInfo TryParseHtmlTagsFromSelf(this LinkInfo result, HtmlDocument document, bool includeDescription)
		{
			//there are still sites out there that are not using og: meta tags
			//trying to get preview values from html tags
			//in case of images, trying to get them either from self or select the biggest image that is one the page, with some filters applied
			if (string.IsNullOrWhiteSpace(result.Title))
			{
				result.Title = document.TryGetTitleFromSelf();
			}

			if (includeDescription)
				if (string.IsNullOrWhiteSpace(result.Description))
				{
					result.Description = document.TryGetDescriptionFromSelf();
				}

			if (result.ImageUrl == null)
			{
				result.ImageUrl = document.TryGetImageUrlFromSelf();

				if (result.ImageUrl == null)
				{
					result.ImageUrl = document.TryGetImageUrlFromContent();
				}
			}

			return result;
		}

		private static LinkInfo TryGetCanonicalUrl(this LinkInfo result, HtmlDocument document)
		{
			var links = document.DocumentNode.SelectNodes("//link");

			if (links != null)
			{
				string canonical = null;

				foreach (HtmlNode link in links)
					if (link.GetAttributeValue("rel", null) != "canonical")
						continue;
					else
						canonical = link.GetAttributeValue("href", null);

				if (!string.IsNullOrWhiteSpace(canonical))
				{
					if (!canonical.StartsWith("/"))
					{
						var canonicalUri = new Uri(canonical);

						//in 99.9 % of all cases, the first segment is always '/'
						//if it has no segments it is an implicit redirect to the home page, which I do not want
						//if it has more segments, chances are high we have a valid canonical url
						//bad guy example => 9gag
						if (canonicalUri.Segments.Length > 1)
							result.CanoncialUrl = canonicalUri;
					}
				}
			}

			return result;
		}


		private static string TryGetTitleFromSelf(this HtmlDocument document)
		{
			string result = null;
			var titleNodes = document.DocumentNode.SelectNodes("//title");
			if (titleNodes != null)
				result = titleNodes.FirstOrDefault()?.InnerText;

			return result;
		}

		private static string TryGetDescriptionFromSelf(this HtmlDocument document)
		{
			string result = null;
			var description = document.DocumentNode.SelectNodes("//description");
			if (description != null)
				result = description.FirstOrDefault()?.InnerText;

			return result;
		}

		private static Uri TryGetImageUrlFromSelf(this HtmlDocument document)
		{
			Uri result = null;
			var linkNodes = document.DocumentNode.SelectNodes("//link");
			if (linkNodes != null)
			{
				foreach (var link in linkNodes)
					if (link.GetAttributeValue("rel", null) != "image_src")
						continue;
					else
					{
						var url = link.GetAttributeValue("href", null);
						if (!string.IsNullOrWhiteSpace(url))
							result = new Uri(url);
					}
			}
			return result;
		}

		public static Uri TryGetImageUrlFromContent(this HtmlDocument document)
		{
			var allImgs = document.DocumentNode.Descendants("img").Where(n => !string.IsNullOrWhiteSpace(n.GetAttributeValue("src", null))).ToList();

			var imgsWithSize = allImgs.Where(n => n.GetAttributeValue("width", null) != null && n.GetAttributeValue("height", null) != null).ToList();

			var imgsBigEnough = new List<HtmlNode>();

			foreach (var img in imgsWithSize)
			{
				var height = Convert.ToInt32(img.Attributes["height"].Value);
				var width = Convert.ToInt32(img.Attributes["width"].Value);

				if (height <= 50 || width <= 50)
					continue;

				if (width > height)
				{
					if (width / height > 3)
						continue;
				}
				else
				{
					if (height / width > 3)
						continue;
				}

				imgsBigEnough.Add(img);
			}

			if (imgsBigEnough.Any())
			{
				var orderImgsBigEnoughBySize = imgsBigEnough.OrderByDescending(n => Convert.ToInt32(n.Attributes["width"].Value)).ThenByDescending(n => Convert.ToInt32(n.Attributes["height"].Value)).ToList();

				var imgUrl = orderImgsBigEnoughBySize.FirstOrDefault().GetAttributeValue("src", null);

				return !string.IsNullOrWhiteSpace(imgUrl) ? null : new Uri(imgUrl);
			}

			return null;
		}


		private static LinkInfo TryParseSchemaOrgFromSelf(this LinkInfo result, HtmlDocument document, bool includeDescription)
		{
			var scriptNodesWithType = document.DocumentNode.SelectNodes("//script").
						Where(n => !string.IsNullOrWhiteSpace(n.GetAttributeValue("type", null))).ToList();

			var applicationLdJsonNodes = scriptNodesWithType
								.Where(n => n.Attributes["type"].Value.Contains("application/ld+json")).ToList();

			List<JObject> schemaOrgNodes = new List<JObject>();

			//some websites implement schema.org as single entity, while others use an array
			//this way, we are catching them all!
			try
			{
				foreach (var node in applicationLdJsonNodes)
				{
					var nodeObject = JObject.Parse(node.InnerText);

					if (nodeObject != null)
						schemaOrgNodes.Add(nodeObject);
				}

			}
			catch (Exception ex)
			{
				if (ex is JsonReaderException)
				{
					try
					{
						var schemaOrgArrays = applicationLdJsonNodes.Select(n => JArray.Parse(n.InnerText)).ToList();

						foreach (var array in schemaOrgArrays)
						{
							schemaOrgNodes.Add((JObject)array.FirstOrDefault());
						}
					}
					catch (Exception ex2)
					{
						if (ex2 is JsonReaderException)
						{
							if (!schemaOrgNodes.Any())
								return result;
						}
					}
				}
			}

			var articleInfo = schemaOrgNodes.FirstOrDefault(jo => ((string)jo.SelectToken("@type")).IsArticleOrSubtype());

			if (articleInfo != null)
			{
				string headline = (string)articleInfo.SelectToken("headline");
				if (!string.IsNullOrWhiteSpace(headline))
					result.Title = headline;


				string imageUrl = (string)articleInfo.SelectToken("image")?.SelectToken("url");
				if (!string.IsNullOrWhiteSpace(imageUrl))
					result.ImageUrl = new Uri(imageUrl);
				else
				{
					string thumbnailUrl = (string)articleInfo.SelectToken("thumbnailUrl");
					if (!string.IsNullOrWhiteSpace(thumbnailUrl))
						result.ImageUrl = new Uri(thumbnailUrl);
				}

				string mainEntityOfPage = null;
				try
				{
					mainEntityOfPage = (string)articleInfo.SelectToken("mainEntityOfPage");
				}
				catch (Exception ex)
				{
					if (ex is ArgumentException)
					{
						mainEntityOfPage = (string)articleInfo.SelectToken("mainEntityOfPage")?.SelectToken("@id");
					}
				}

				if (!string.IsNullOrWhiteSpace(mainEntityOfPage))
					result.CanoncialUrl = new Uri(mainEntityOfPage);


				if (includeDescription)
				{
					string description = (string)articleInfo.SelectToken("description");
					if (!string.IsNullOrWhiteSpace(description))
						result.Description = description;
				}
			}

			return result;
		}

		//https://schema.org/Article
		private static bool IsArticleOrSubtype(this string typeString)
		{
			return typeString == "Article" ||
				   typeString == "AdvertiserContentArticle" ||
				   typeString == "NewsArticle" ||
				   typeString == "Report" ||
				   typeString == "SatiricalArticle" ||
				   typeString == "ScholarlyArticle" ||
				   typeString == "MedicalScholarlyArticle" ||
				   typeString == "SocialMediaPosting" ||
				   typeString == "BlogPosting" ||
				   typeString == "LiveBlogPosting" ||
				   typeString == "DiscussionForumPosting" ||
				   typeString == "TechArticle" ||
				   typeString == "ApiReference";
		}
	}
}
