using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosDbSdkClient
{
	public class SdkClientResult
	{
		public RequestInfo RequestInfo { get; set; } = new RequestInfo();

		public object Content { get; set; }
	}
}
