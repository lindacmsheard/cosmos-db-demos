using System;
using System.ComponentModel.Design;
using System.Net.Http;
using CosmosDbRestApiClient;

namespace CosmosDbRestApiClient.App
{
	class Program
	{
		const string COSMOS_DB_ENDPOINT = "Cosmos-DB-Endpoint";
		const string COSMOS_DB_KEY = "Cosmos-DB-Key";

		static string _databaseId = "db1";
		static string _collectionId = "c1";
		static string _documentId = "1";
		static string _partitionKey = "green";

		private static void Initialize()
		{
			Environment.SetEnvironmentVariable(COSMOS_DB_ENDPOINT, "PROVIDE", EnvironmentVariableTarget.Process);
			Environment.SetEnvironmentVariable(COSMOS_DB_KEY, "PROVIDE", EnvironmentVariableTarget.Process);
		}

		static void Main(string[] args)
		{
			// Demo environment
			Initialize();

			// Get these values from environment vars - could do this in an Azure App to retrieve from App Settings
			string endpoint = Environment.GetEnvironmentVariable(COSMOS_DB_ENDPOINT);
			string primaryKey = Environment.GetEnvironmentVariable(COSMOS_DB_KEY);

			ApiClient apiClient = new ApiClient(endpoint, primaryKey);

			HttpResponseMessage httpResponseMessage;

			httpResponseMessage = apiClient.ListDatabasesAsync().Result;
			WriteOut("List Databases", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			httpResponseMessage = apiClient.GetDatabaseAsync(_databaseId).Result;
			WriteOut($"Get Database {_databaseId}", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			httpResponseMessage = apiClient.ListCollectionsAsync(_databaseId).Result;
			WriteOut("List Collections", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			httpResponseMessage = apiClient.GetCollectionAsync(_databaseId, _collectionId).Result;
			WriteOut($"Get Collection {_collectionId}", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			httpResponseMessage = apiClient.ListDocumentsAsync(_databaseId, _collectionId).Result;
			WriteOut("List Documents", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			httpResponseMessage = apiClient.GetDocumentAsync(_databaseId, _collectionId, _documentId, _partitionKey).Result;
			WriteOut($"Get Document {_documentId} with partition key {_partitionKey}", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			var newdoc = new { id = "10", foo = "bar", partitionKey = _partitionKey };
			httpResponseMessage = apiClient.UpsertAsync(_databaseId, _collectionId, newdoc, _partitionKey).Result;
			WriteOut("Upsert", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			string query = $"SELECT * FROM c WHERE c.partitionKey = \"{_partitionKey}\"";
			httpResponseMessage = apiClient.QueryAsync(_databaseId, _collectionId, query).Result;
			WriteOut($"Query: {query}", apiClient.GetHttpResponseHeaders(httpResponseMessage, true), apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		static void WriteOut(string title, string headers, string content)
		{
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine(title);
			Console.WriteLine();
			Console.WriteLine(headers);
			Console.WriteLine();
			Console.WriteLine(content);
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
