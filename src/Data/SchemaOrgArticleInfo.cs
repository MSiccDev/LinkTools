using System;
using Newtonsoft.Json;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class SchemaOrgArticleInfo
	{
		[JsonProperty("@context")]
		public Uri Context { get; set; }

		[JsonProperty("@type")]
		public string Type { get; set; }

		[JsonProperty("headline")]
		public string Headline { get; set; }

		[JsonProperty("datePublished")]
		public DateTimeOffset DatePublished { get; set; }

		[JsonProperty("dateModified")]
		public DateTimeOffset DateModified { get; set; }

		[JsonProperty("isAccessibleForFree")]
		public string IsAccessibleForFree { get; set; }

		[JsonProperty("isPartOf")]
		public SchemaOrgIsPartOf IsPartOf { get; set; }

		[JsonProperty("image")]
		public SchemaOrgImageInfo Image { get; set; }

		[JsonProperty("author")]
		public SchemaOrgAuthorInfo Author { get; set; }

		[JsonProperty("articleBody")]
		public string ArticleBody { get; set; }

		[JsonProperty("wordCount")]
		public long WordCount { get; set; }

		[JsonProperty("publisher")]
		public SchemaOrgPublisherInfo Publisher { get; set; }
	}
}

