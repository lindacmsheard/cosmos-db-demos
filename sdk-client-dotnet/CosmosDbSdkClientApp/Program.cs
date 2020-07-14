using System;
using System.Collections.Generic;
using System.Linq;
using CosmosDbSdkClient;

namespace CosmosDbSdkClientApp
{
	class Program
	{
		const string COSMOS_DB_ENDPOINT = "CosmosDBEndpoint";
		const string COSMOS_DB_KEY = "CosmosDBKey";

		static string _databaseId = "db1";
		static string _collectionId = "c1";
		static string _documentId = "30";
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

			SdkClientResult listDatabasesResult = sdkClient.ListDatabasesAsync().Result;
			WriteOut("List Databases", listDatabasesResult.RequestInfo.AsJson(true), listDatabasesResult.Content.AsJson(true));

			SdkClientResult getDatabaseResult = sdkClient.GetDatabaseAsync(_databaseId).Result;
			WriteOut($"Get Database {_databaseId}", getDatabaseResult.RequestInfo.AsJson(true), getDatabaseResult.Content.AsJson(true));

			SdkClientResult listCollectionsResult = sdkClient.ListCollectionsAsync(_databaseId).Result;
			WriteOut("List Collections", listCollectionsResult.RequestInfo.AsJson(true), listCollectionsResult.Content.AsJson(true));

			SdkClientResult getCollectionResult = sdkClient.GetCollectionAsync(_databaseId, _collectionId).Result;
			WriteOut($"Get Collection {_collectionId}", getCollectionResult.RequestInfo.AsJson(true), getCollectionResult.Content.AsJson(true));

			SdkClientResult listDocumentsResult = sdkClient.ListDocumentsAsync(_databaseId, _collectionId).Result;
			WriteOut("List Documents", listDocumentsResult.RequestInfo.AsJson(true), listDocumentsResult.Content.AsJson(true));

			SdkClientResult<SampleItem> getDocumentResult = sdkClient.GetDocumentAsync<SampleItem>(_databaseId, _collectionId, _documentId, _partitionKey).Result;
			WriteOut($"Get Document {_documentId} with partition key {_partitionKey}", getDocumentResult.RequestInfo.AsJson(true), getDocumentResult.Content.AsJson(true));

			SampleItem newDoc = GetNewDocument();
			SdkClientResult<SampleItem> upsertResult = sdkClient.UpsertAsync<SampleItem>(_databaseId, _collectionId, newDoc, _partitionKey).Result;
			WriteOut("Upsert", upsertResult.RequestInfo.AsJson(true), upsertResult.Content.AsJson(true));

			string query = $"SELECT * FROM c WHERE c.partitionKey = \"{_partitionKey}\"";
			SdkClientResult<List<SampleItem>> queryResult = sdkClient.QueryAsync<SampleItem>(_databaseId, _collectionId, query).Result;
			WriteOut($"Query: {query}", queryResult.RequestInfo.AsJson(true), queryResult.Content.AsJson(true));

			string sprocName = "getItem";
			SdkClientResult<SampleItem> sprocResult = sdkClient.ExecSprocAsync<SampleItem>(_databaseId, _collectionId, sprocName, _documentId, _partitionKey).Result;
			WriteOut($"Sproc: {sprocName}", sprocResult.RequestInfo.AsJson(true), sprocResult.Content.AsJson(true));

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

		static SampleItem GetNewDocument()
		{
			return new SampleItem
			{
				Id = _documentId,
				Foo = Guid.NewGuid().ToString(),
				PartitionKey = _partitionKey,
				Amount = 1.11
			};
		}
	}
}
