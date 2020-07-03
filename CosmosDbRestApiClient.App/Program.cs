using System;
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

		static void Main(string[] args)
		{
			ApiClient apiClient = new ApiClient(_endpoint, _primaryKey);

			Console.WriteLine("List Databases");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.ListDatabasesAsync().Result).Result);
			Console.WriteLine();

			Console.WriteLine($"Get Database {_databaseId}");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.GetDatabaseAsync(_databaseId).Result).Result);
			Console.WriteLine();

			Console.WriteLine("List Collections");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.ListCollectionsAsync(_databaseId).Result).Result);
			Console.WriteLine();

			Console.WriteLine($"Get Collection {_collectionId}");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.GetCollectionAsync(_databaseId, _collectionId).Result).Result);
			Console.WriteLine();

			Console.WriteLine("List Documents");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.ListDocumentsAsync(_databaseId, _collectionId).Result).Result);
			Console.WriteLine();

			Console.WriteLine($"Get Document {_documentId} with partition key {_partitionKey}");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.GetDocumentAsync(_databaseId, _collectionId, _documentId, _partitionKey).Result).Result);
			Console.WriteLine();

			string query = $"SELECT * FROM c WHERE c.partitionKey = \"{_partitionKey}\"";
			Console.WriteLine("Query");
			Console.WriteLine(apiClient.GetHttpResponseContentAsync(apiClient.QueryAsync(_databaseId, _collectionId, query).Result).Result);
			Console.WriteLine();

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}

	}
}
