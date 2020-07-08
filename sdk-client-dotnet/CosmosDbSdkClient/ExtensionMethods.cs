using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace CosmosDbSdkClient
{
	public static class ExtensionMethods
	{
		public static string AsJson(this object serializeMe, bool asFormattedJson = false)
		{
			return JsonConvert.SerializeObject(serializeMe, (asFormattedJson ? Formatting.Indented : Formatting.None));
		}
	}
}
