using System;
using Newtonsoft.Json;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class SchemaOrgAuthorInfo
	{
		[JsonProperty("@type")]
		public string Type { get; set; }

		[JsonProperty("@context")]
		public Uri Context { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("url")]
		public Uri Url { get; set; }

		[JsonProperty("height")]
		public string Height { get; set; }

		[JsonProperty("worksFor")]
		public SchemaOrgPublisherInfo WorksFor { get; set; }
	}
}