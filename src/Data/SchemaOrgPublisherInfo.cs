using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class SchemaOrgPublisherInfo
	{
		[JsonProperty("@type")]
		public string Type { get; set; }

		[JsonProperty("@context")]
		public Uri Context { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("legalName")]
		public string LegalName { get; set; }

		[JsonProperty("logo")]
		public SchemaOrgImageInfo Logo { get; set; }

		[JsonProperty("url")]
		public Uri Url { get; set; }

		[JsonProperty("sameAs")]
		public List<Uri> SameAs { get; set; }
	}
}

