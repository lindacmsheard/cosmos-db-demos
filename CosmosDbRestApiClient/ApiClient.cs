using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CosmosDbRestApiClient
{
	public class ApiClient
	{
		#region Enums

		public enum CosmosDbResourceType
		{
			dbs,
			colls,
			docs
		}

		#endregion

		#region Constants

		public const string API_VERSION = "2018-12-31";  // API version from https://docs.microsoft.com/rest/api/cosmos-db/

		public const string DATE_HEADER = "x-ms-date";
		public const string API_VERSION_HEADER = "x-ms-version";
		public const string AUTHORIZATION_HEADER = "authorization";

		public const string PARTITION_KEY_HEADER = "x-ms-documentdb-partitionkey";

		public const string QUERY_HEADER = "x-ms-documentdb-isquery";
		public const string CROSS_PARTITION_HEADER = "x-ms-documentdb-query-enablecrosspartition";

		public const string CONTENT_TYPE_QUERY = "application/query+json";

		#endregion

		#region Variables

		private string _apiVersion = string.Empty;
		private string _utcDate = string.Empty;

		private AuthTokenCreator _authTokenCreator = null;

		private Uri _baseUri = null;

		#endregion

		#region Properties

		public string ApiVersion
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_apiVersion))
					_apiVersion = API_VERSION;

				return _apiVersion;
			}
			set
			{
				_apiVersion = value;
			}
		}

		public string UtcDate
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_utcDate))
					_utcDate = DateTime.UtcNow.ToString("R").ToLowerInvariant();

				return _utcDate;
			}
			set
			{
				_utcDate = value.ToLowerInvariant();
			}
		}

		public AuthTokenCreator AuthTokenCreator
		{
			get
			{
				if (_authTokenCreator == null)
					_authTokenCreator = new AuthTokenCreator();

				return _authTokenCreator;
			}
		}

		public string CosmosDbEndpoint { get; set; }

		public string CosmosDbKey { get; set; }

		public Uri CosmosDbEndpointUri
		{
			get
			{
				if (_baseUri == null)
					_baseUri = new Uri(this.CosmosDbEndpoint);

				return _baseUri;
			}
		}

		#endregion

		#region Constructors

		private ApiClient()
		{

		}

		public ApiClient(string cosmosDbEndpoint, string cosmosDbKey) : this()
		{
			this.CosmosDbEndpoint = cosmosDbEndpoint;
			this.CosmosDbKey = cosmosDbKey;
		}

		#endregion

		public async Task<HttpResponseMessage> ListDatabasesAsync()
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.dbs;

			string resourceId = string.Empty;
			string resourceLink = $"{resourceType}";

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessAsync(authToken, resourceLink);
		}

		public async Task<HttpResponseMessage> GetDatabaseAsync(string databaseId)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.dbs;

			string resourceId = $"{resourceType}/{databaseId}";
			string resourceLink = resourceId;

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessAsync(authToken, resourceLink);
		}

		public async Task<HttpResponseMessage> ListCollectionsAsync(string databaseId)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.colls;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}";
			string resourceLink = $"{resourceId}/{resourceType}";

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessAsync(authToken, resourceLink);
		}

		public async Task<HttpResponseMessage> GetCollectionAsync(string databaseId, string collectionId)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.colls;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}/{resourceType}/{collectionId}";
			string resourceLink = resourceId;

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessAsync(authToken, resourceLink);
		}

		public async Task<HttpResponseMessage> ListDocumentsAsync(string databaseId, string collectionId)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.docs;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}/{CosmosDbResourceType.colls}/{collectionId}";
			string resourceLink = $"{resourceId}/{resourceType}";

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessAsync(authToken, resourceLink);
		}

		public async Task<HttpResponseMessage> GetDocumentAsync(string databaseId, string collectionId, string documentId, string partitionKey)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.docs;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}/{CosmosDbResourceType.colls}/{collectionId}/{CosmosDbResourceType.docs}/{documentId}";
			string resourceLink = resourceId;

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Get, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessPointReadAsync(authToken, resourceLink, partitionKey);
		}

		public async Task<HttpResponseMessage> QueryAsync(string databaseId, string collectionId, string query)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.docs;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}/{CosmosDbResourceType.colls}/{collectionId}";
			string resourceLink = $"{resourceId}/{resourceType}";

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Post, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessQueryAsync(authToken, resourceLink, query);
		}

		public async Task<string> GetHttpResponseContentAsync(HttpResponseMessage httpResponseMessage)
		{
			if (httpResponseMessage == null)
				return string.Empty;

			return await httpResponseMessage.Content.ReadAsStringAsync();
		}

		/// <summary>
		///  The Cosmos DB REST API expects a partition key to be passed as follows: [ "partitionKey" ]
		/// </summary>
		/// <param name="partitionKey"></param>
		/// <returns></returns>
		private string FormatPartitionKeyForRestApiHeader(string partitionKey)
		{
			if (!string.IsNullOrWhiteSpace(partitionKey))
				return $"[\"{partitionKey}\"]";
			else
				return string.Empty;
		}

		/// <summary>
		/// Prepares HTTP body content that can be used with POST
		/// </summary>
		/// <param name="contents">Appropriately serialized HTTP body content</param>
		/// <param name="mediaType">HTTP media type expected by the URL, like "application/json" or "application/xml"</param>
		/// <returns></returns>
		private HttpContent GetHttpContent(string contents, string mediaType)
		{
			Encoding encoding = Encoding.UTF8;

			HttpContent result = new StringContent(contents, encoding, mediaType);

			result.Headers.ContentType.CharSet = string.Empty;

			return result;
		}

		private string FormatQueryForRestApi(string rawQuery)
		{
			return JsonConvert.SerializeObject(new { query = rawQuery });
		}

		private async Task<HttpResponseMessage> ProcessAsync(string authToken, string resourceLink)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add(DATE_HEADER, this.UtcDate);
			client.DefaultRequestHeaders.Add(API_VERSION_HEADER, this.ApiVersion);

			client.DefaultRequestHeaders.Remove(AUTHORIZATION_HEADER);
			client.DefaultRequestHeaders.Add(AUTHORIZATION_HEADER, authToken);

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);

			return httpResponseMessage;
		}

		private async Task<HttpResponseMessage> ProcessPointReadAsync(string authToken, string resourceLink, string partitionKey)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add(DATE_HEADER, this.UtcDate);
			client.DefaultRequestHeaders.Add(API_VERSION_HEADER, this.ApiVersion);

			if (!string.IsNullOrWhiteSpace(partitionKey))
				client.DefaultRequestHeaders.Add(PARTITION_KEY_HEADER, FormatPartitionKeyForRestApiHeader(partitionKey));

			client.DefaultRequestHeaders.Remove(AUTHORIZATION_HEADER);
			client.DefaultRequestHeaders.Add(AUTHORIZATION_HEADER, authToken);

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead);

			return httpResponseMessage;
		}

		private async Task<HttpResponseMessage> ProcessQueryAsync(string authToken, string resourceLink, string query)
		{
			var client = new HttpClient();

			client.DefaultRequestHeaders.Add(DATE_HEADER, this.UtcDate);
			client.DefaultRequestHeaders.Add(API_VERSION_HEADER, this.ApiVersion);

			client.DefaultRequestHeaders.Add(QUERY_HEADER, "true");
			client.DefaultRequestHeaders.Add(CROSS_PARTITION_HEADER, "true");

			client.DefaultRequestHeaders.Remove(AUTHORIZATION_HEADER);
			client.DefaultRequestHeaders.Add(AUTHORIZATION_HEADER, authToken);

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await client.PostAsync(uri, GetHttpContent(FormatQueryForRestApi(query), CONTENT_TYPE_QUERY));

			return httpResponseMessage;
		}
	}
}
