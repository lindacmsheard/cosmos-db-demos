using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosDbSdkClientApp
{
	public class SampleItem
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("partitionKey")]
		public string PartitionKey { get; set; }

		[JsonProperty("foo")]
		public string Foo { get; set; }

		[JsonProperty("amount")]
		public double Amount { get; set; }

		[JsonProperty("retailPrice")]
		public double RetailPrice { get; set; }

		[JsonProperty("_ts")]
		public long Timestamp { get; set; }
	}
}
