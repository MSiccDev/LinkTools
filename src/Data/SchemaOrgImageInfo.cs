using System;
using Newtonsoft.Json;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class SchemaOrgImageInfo
	{
		[JsonProperty("@type")]
		public string Type { get; set; }

		[JsonProperty("@context")]
		public Uri Context { get; set; }

		[JsonProperty("url")]
		public Uri Url { get; set; }

		[JsonProperty("width")]
		public long Width { get; set; }

		[JsonProperty("height")]
		public long Height { get; set; }
	}
}

