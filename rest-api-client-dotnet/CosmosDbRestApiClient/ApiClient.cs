using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using pelazem.http;

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

		public const string UPSERT_HEADER = "x-ms-documentdb-is-upsert";

		public const string CONTENT_TYPE_QUERY = "application/query+json";
		public const string CONTENT_TYPE_DOCUMENT = "application/json";

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

		private HttpUtil HttpUtil { get; } = new HttpUtil();

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

		#region Operations

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

		public async Task<HttpResponseMessage> UpsertAsync(string databaseId, string collectionId, dynamic document, string partitionKey)
		{
			CosmosDbResourceType resourceType = CosmosDbResourceType.docs;

			string resourceId = $"{CosmosDbResourceType.dbs}/{databaseId}/{CosmosDbResourceType.colls}/{collectionId}";
			string resourceLink = $"{resourceId}/{resourceType}";

			string authToken = this.AuthTokenCreator.GenerateAuthToken(HttpMethod.Post, resourceType, resourceId, this.CosmosDbKey, this.UtcDate);

			return await ProcessUpsertAsync(authToken, resourceLink, document, partitionKey);
		}

		#endregion

		#region Utility

		public async Task<string> GetHttpResponseContentAsync(HttpResponseMessage httpResponseMessage, bool asFormattedJson = false)
		{
			if (httpResponseMessage?.Content == null)
				return string.Empty;

			string raw = await httpResponseMessage.Content.ReadAsStringAsync();

			if (!asFormattedJson)
				return raw;
			else
			{
				dynamic parsed = JsonConvert.DeserializeObject(raw);
				return AsJson(parsed, true);
			}
		}

		public string GetHttpResponseHeaders(HttpResponseMessage httpResponseMessage, bool asFormattedJson = false)
		{
			if (httpResponseMessage?.Headers == null || httpResponseMessage.Headers.Count() == 0)
				return string.Empty;

			string result = string.Empty;

			if (!asFormattedJson)
			{
				foreach (var header in httpResponseMessage.Headers)
					result += header.Key + " = " + header.Value + Environment.NewLine;
			}
			else
				result = AsJson(httpResponseMessage.Headers, true);

			return result;
		}

		public string AsJson(object serializeMe, bool asFormattedJson = false)
		{
			return JsonConvert.SerializeObject(serializeMe, (asFormattedJson ? Formatting.Indented : Formatting.None));
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
		private HttpContent CreateHttpContent(string contents, string mediaType)
		{
			Encoding encoding = Encoding.UTF8;

			HttpContent result = new StringContent(contents, encoding, mediaType);

			// Required by Cosmos DB REST API
			result.Headers.ContentType.CharSet = string.Empty;

			return result;
		}

		private string FormatQueryForRestApi(string rawQuery)
		{
			return JsonConvert.SerializeObject(new { query = rawQuery });
		}

		private void CleanRequestHeaders()
		{
			this.HttpUtil.RemoveRequestHeader(DATE_HEADER);
			this.HttpUtil.RemoveRequestHeader(API_VERSION_HEADER);
			this.HttpUtil.RemoveRequestHeader(AUTHORIZATION_HEADER);
			this.HttpUtil.RemoveRequestHeader(PARTITION_KEY_HEADER);
			this.HttpUtil.RemoveRequestHeader(QUERY_HEADER);
			this.HttpUtil.RemoveRequestHeader(CROSS_PARTITION_HEADER);

		}

		#endregion

		#region API Interaction

		private async Task<HttpResponseMessage> ProcessAsync(string authToken, string resourceLink)
		{
			CleanRequestHeaders();

			this.HttpUtil.AddRequestHeader(DATE_HEADER, this.UtcDate);
			this.HttpUtil.AddRequestHeader(API_VERSION_HEADER, this.ApiVersion);
			this.HttpUtil.AddRequestHeader(AUTHORIZATION_HEADER, authToken);

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await this.HttpUtil.HttpClient.GetAsync(uri);

			return httpResponseMessage;
		}

		private async Task<HttpResponseMessage> ProcessPointReadAsync(string authToken, string resourceLink, string partitionKey)
		{
			CleanRequestHeaders();

			this.HttpUtil.AddRequestHeader(DATE_HEADER, this.UtcDate);
			this.HttpUtil.AddRequestHeader(API_VERSION_HEADER, this.ApiVersion);
			this.HttpUtil.AddRequestHeader(AUTHORIZATION_HEADER, authToken);

			if (!string.IsNullOrWhiteSpace(partitionKey))
				this.HttpUtil.AddRequestHeader(PARTITION_KEY_HEADER, FormatPartitionKeyForRestApiHeader(partitionKey));

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await this.HttpUtil.HttpClient.GetAsync(uri);

			return httpResponseMessage;
		}

		private async Task<HttpResponseMessage> ProcessQueryAsync(string authToken, string resourceLink, string query)
		{
			CleanRequestHeaders();

			this.HttpUtil.AddRequestHeader(DATE_HEADER, this.UtcDate);
			this.HttpUtil.AddRequestHeader(API_VERSION_HEADER, this.ApiVersion);
			this.HttpUtil.AddRequestHeader(AUTHORIZATION_HEADER, authToken);

			this.HttpUtil.AddRequestHeader(QUERY_HEADER, "true");
			this.HttpUtil.AddRequestHeader(CROSS_PARTITION_HEADER, "true");

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await this.HttpUtil.HttpClient.PostAsync(uri, CreateHttpContent(FormatQueryForRestApi(query), CONTENT_TYPE_QUERY));

			return httpResponseMessage;
		}

		private async Task<HttpResponseMessage> ProcessUpsertAsync(string authToken, string resourceLink, dynamic document, string partitionKey)
		{
			CleanRequestHeaders();

			this.HttpUtil.AddRequestHeader(DATE_HEADER, this.UtcDate);
			this.HttpUtil.AddRequestHeader(API_VERSION_HEADER, this.ApiVersion);
			this.HttpUtil.AddRequestHeader(AUTHORIZATION_HEADER, authToken);

			if (!string.IsNullOrWhiteSpace(partitionKey))
				this.HttpUtil.AddRequestHeader(PARTITION_KEY_HEADER, FormatPartitionKeyForRestApiHeader(partitionKey));

			this.HttpUtil.AddRequestHeader(UPSERT_HEADER, "true");

			string doc = JsonConvert.SerializeObject(document);

			Uri uri = new Uri(this.CosmosDbEndpointUri, resourceLink);
			HttpResponseMessage httpResponseMessage = await this.HttpUtil.HttpClient.PostAsync(uri, CreateHttpContent(doc, CONTENT_TYPE_DOCUMENT));

			return httpResponseMessage;
		}

		#endregion
	}
}
