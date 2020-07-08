using System;
using System.Net.Http;
using CosmosDbRestApiClient;

namespace CosmosDbRestApiClient.App
{
	class Program
	{
		static string _endpoint = "https://PROVIDE-SQL-ACCOUNT-NAME.documents.azure.com/";
		static string _primaryKey = "PROVIDE";
		static string _databaseId = "db1";
		static string _collectionId = "c1";
		static string _documentId = "1";
		static string _partitionKey = "green";

		static ApiClient _apiClient = new ApiClient(_endpoint, _primaryKey);

		static void Main(string[] args)
		{
			WriteOut("List Databases", _apiClient.ListDatabasesAsync().Result);

			WriteOut($"Get Database {_databaseId}", _apiClient.GetDatabaseAsync(_databaseId).Result);

			WriteOut("List Collections", _apiClient.ListCollectionsAsync(_databaseId).Result);

			WriteOut($"Get Collection {_collectionId}", _apiClient.GetCollectionAsync(_databaseId, _collectionId).Result);

			WriteOut("List Documents", _apiClient.ListDocumentsAsync(_databaseId, _collectionId).Result);

			WriteOut($"Get Document {_documentId} with partition key {_partitionKey}", _apiClient.GetDocumentAsync(_databaseId, _collectionId, _documentId, _partitionKey).Result);

			var newdoc = new { id = "10", foo = "bar", partitionKey = _partitionKey };
			WriteOut("Upsert", _apiClient.UpsertAsync(_databaseId, _collectionId, newdoc, _partitionKey).Result);

			string query = $"SELECT * FROM c WHERE c.partitionKey = \"{_partitionKey}\"";
			WriteOut($"Query: {query}", _apiClient.QueryAsync(_databaseId, _collectionId, query).Result);

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

		static void WriteOut(string title, HttpResponseMessage httpResponseMessage)
		{
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine(title);
			Console.WriteLine();
			Console.WriteLine(_apiClient.GetHttpResponseHeaders(httpResponseMessage, true));
			Console.WriteLine();
			Console.WriteLine(_apiClient.GetHttpResponseContentAsync(httpResponseMessage, true).Result);
			Console.WriteLine("--------------------------------------------------");
			Console.WriteLine();
			Console.WriteLine();
		}
	}
}
