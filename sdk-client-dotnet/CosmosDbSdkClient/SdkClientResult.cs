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

		public virtual object Content { get; set; }
	}

	public class SdkClientResult<T> : SdkClientResult
	{
		public new T Content { get; set; }
	}
}
