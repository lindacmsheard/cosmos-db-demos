using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CosmosDbRestApiClient
{
	public class AuthTokenCreator
	{
		public const string KEYTYPE = "master";

		/// <summary>
		/// Based on https://github.com/Azure/azure-cosmos-dotnet-v2/blob/master/samples/rest-from-.net/Program.cs
		/// </summary>
		/// <param name="httpMethod"></param>
		/// <param name="resourceType"></param>
		/// <param name="resourceId"></param>
		/// <param name="cosmosDbKey"></param>
		/// <param name="utcDate">RFC 7231 (HTTP-Date) formatted date</param>
		/// <param name="tokenVersion"></param>
		/// <returns></returns>
		public string GenerateAuthToken(HttpMethod httpMethod, ApiClient.CosmosDbResourceType resourceType, string resourceId, string cosmosDbKey, string utcDate, string tokenVersion = "1.0")
		{
			var hmacSha256 = new HMACSHA256 { Key = Convert.FromBase64String(cosmosDbKey) };

			string payLoad = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\n{1}\n{2}\n{3}\n{4}\n",
					httpMethod.Method.ToLowerInvariant(),
					resourceType.ToString().ToLowerInvariant(),
					resourceId,
					utcDate,
					string.Empty
			);

			byte[] hashPayLoad = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(payLoad));
			string signature = Convert.ToBase64String(hashPayLoad);

			return HttpUtility.UrlEncode
			(String.Format(CultureInfo.InvariantCulture, "type={0}&ver={1}&sig={2}",
				KEYTYPE,
				tokenVersion,
				signature)
			);
		}
	}
}
