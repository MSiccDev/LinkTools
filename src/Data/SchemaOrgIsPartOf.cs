using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MSiccDev.Libs.LinkTools.LinkPreview
{
	public class SchemaOrgIsPartOf
	{
		[JsonProperty("@type")]
		public List<string> Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("productID")]
		public string ProductId { get; set; }
	}
}

