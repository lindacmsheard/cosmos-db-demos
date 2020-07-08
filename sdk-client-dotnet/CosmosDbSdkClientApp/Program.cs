using System;
using CosmosDbSdkClient;

namespace CosmosDbSdkClientApp
{
	class Program
	{
		const string COSMOS_DB_ENDPOINT = "CosmosDBEndpoint";
		const string COSMOS_DB_KEY = "CosmosDBKey";

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

			SdkClient sdkClient = new SdkClient(endpoint, primaryKey);

			SdkClientResult sdkClientResult;

			sdkClientResult = sdkClient.ListDatabasesAsync().Result;
			WriteOut("List Databases", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			sdkClientResult = sdkClient.GetDatabaseAsync(_databaseId).Result;
			WriteOut($"Get Database {_databaseId}", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			sdkClientResult = sdkClient.ListCollectionsAsync(_databaseId).Result;
			WriteOut("List Collections", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			sdkClientResult = sdkClient.GetCollectionAsync(_databaseId, _collectionId).Result;
			WriteOut($"Get Collection {_collectionId}", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			sdkClientResult = sdkClient.ListDocumentsAsync(_databaseId, _collectionId).Result;
			WriteOut("List Documents", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			sdkClientResult = sdkClient.GetDocumentAsync(_databaseId, _collectionId, _documentId, _partitionKey).Result;
			WriteOut($"Get Document {_documentId} with partition key {_partitionKey}", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			var newdoc = new { id = "10", foo = "baz", partitionKey = _partitionKey };
			sdkClientResult = sdkClient.UpsertAsync(_databaseId, _collectionId, newdoc, _partitionKey).Result;
			WriteOut("Upsert", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

			string query = $"SELECT * FROM c WHERE c.partitionKey = \"{_partitionKey}\"";
			sdkClientResult = sdkClient.QueryAsync(_databaseId, _collectionId, query).Result;
			WriteOut($"Query: {query}", sdkClientResult.RequestInfo.AsJson(true), sdkClientResult.Content.AsJson(true));

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
