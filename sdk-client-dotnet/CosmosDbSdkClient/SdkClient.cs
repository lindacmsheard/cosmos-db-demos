﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json;

namespace CosmosDbSdkClient
{
	public class SdkClient
	{
		#region Variables

		private CosmosClient _cosmosClient = null;

		#endregion

		#region Properties

		public string CosmosDbEndpoint { get; set; }

		public string CosmosDbKey { get; set; }

		private CosmosClient CosmosClient
		{
			get
			{
				if (_cosmosClient == null)
				{
					CosmosClientOptions clientOptions = new CosmosClientOptions()
					{
						ApplicationRegion = "East US",
						ConnectionMode = ConnectionMode.Gateway
					};

					_cosmosClient = new CosmosClient(this.CosmosDbEndpoint, this.CosmosDbKey, clientOptions);
				}

				return _cosmosClient;
			}
		}

		#endregion

		#region Constructors

		private SdkClient() { }

		public SdkClient(string cosmosDbEndpoint, string cosmosDbKey) : this()
		{
			this.CosmosDbEndpoint = cosmosDbEndpoint;
			this.CosmosDbKey = cosmosDbKey;
		}

		#endregion

		#region Operations

		public async Task<SdkClientResult> ListDatabasesAsync()
		{
			SdkClientResult result = new SdkClientResult();

			List<DatabaseProperties> databases = new List<DatabaseProperties>();

			using (var resultsIterator = this.CosmosClient.GetDatabaseQueryIterator<DatabaseProperties>())
			{
				while (resultsIterator.HasMoreResults)
				{
					FeedResponse<DatabaseProperties> currentResultSet = await resultsIterator.ReadNextAsync();

					databases.AddRange(currentResultSet.Resource);
					result.RequestInfo.RequestCharge += currentResultSet.RequestCharge;
				}
			}

			result.Content = databases;

			return result;
		}

		public async Task<SdkClientResult> GetDatabaseAsync(string databaseId)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy object
			Database database = this.CosmosClient.GetDatabase(databaseId);

			DatabaseResponse response = await database.ReadAsync();

			result.Content = response.Resource;
			result.RequestInfo.RequestCharge = response.RequestCharge;

			return result;
		}

		public async Task<SdkClientResult> ListCollectionsAsync(string databaseId)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy object
			var database = this.CosmosClient.GetDatabase(databaseId);

			List<ContainerProperties> containers = new List<ContainerProperties>();

			using (var resultsIterator = database.GetContainerQueryIterator<ContainerProperties>())
			{
				while (resultsIterator.HasMoreResults)
				{
					FeedResponse<ContainerProperties> currentResultSet = await resultsIterator.ReadNextAsync();

					containers.AddRange(currentResultSet.Resource);
					result.RequestInfo.RequestCharge += currentResultSet.RequestCharge;
				}
			}

			result.Content = containers;

			return result;
		}

		public async Task<SdkClientResult> GetCollectionAsync(string databaseId, string collectionId)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			ContainerResponse response = await container.ReadContainerAsync();

			result.Content = response.Resource;
			result.RequestInfo.RequestCharge = response.RequestCharge;

			return result;
		}

		public async Task<SdkClientResult> ListDocumentsAsync(string databaseId, string collectionId)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			List<object> documents = new List<object>();

			using (var resultsIterator = container.GetItemQueryIterator<object>())
			{
				while (resultsIterator.HasMoreResults)
				{
					FeedResponse<object> currentResultSet = await resultsIterator.ReadNextAsync();

					documents.AddRange(currentResultSet.Resource);
					result.RequestInfo.RequestCharge += currentResultSet.RequestCharge;
				}
			}

			result.Content = documents;

			return result;
		}

		public async Task<SdkClientResult> GetDocumentAsync(string databaseId, string collectionId, string documentId, string partitionKey)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			ItemResponse<object> response = await container.ReadItemAsync<object>(documentId, new PartitionKey(partitionKey));

			result.Content = response.Resource;
			result.RequestInfo.RequestCharge = response.RequestCharge;

			return result;
		}

		public async Task<SdkClientResult> QueryAsync(string databaseId, string collectionId, string query)
		{
			SdkClientResult result = new SdkClientResult();

			QueryDefinition queryDefinition = new QueryDefinition(query);

			QueryRequestOptions queryRequestOptions = new QueryRequestOptions()
			{
				MaxBufferedItemCount = -1,
				MaxConcurrency = -1,
				MaxItemCount = 100
			};

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			List<object> documents = new List<object>();

			using (var resultsIterator = container.GetItemQueryIterator<object>(queryDefinition, requestOptions: queryRequestOptions))
			{
				while (resultsIterator.HasMoreResults)
				{
					FeedResponse<object> currentResultSet = await resultsIterator.ReadNextAsync();

					documents.AddRange(currentResultSet.Resource);
					result.RequestInfo.RequestCharge += currentResultSet.RequestCharge;
				}
			}

			result.Content = documents;

			return result;

		}

		public async Task<SdkClientResult> UpsertAsync(string databaseId, string collectionId, dynamic document, string partitionKey)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			ItemResponse<object> response = await container.UpsertItemAsync<object>(document, new PartitionKey(partitionKey));

			result.Content = response.Resource;
			result.RequestInfo.RequestCharge = response.RequestCharge;

			return result;
		}

		public async Task<SdkClientResult> ExecSprocAsync(string databaseId, string collectionId, string sprocName, string documentId, string partitionKey)
		{
			SdkClientResult result = new SdkClientResult();

			// Local proxy objects
			Database database = this.CosmosClient.GetDatabase(databaseId);
			Container container = database.GetContainer(collectionId);

			StoredProcedureExecuteResponse<string> response = await container.Scripts.ExecuteStoredProcedureAsync<string>(sprocName, new PartitionKey(partitionKey), new[] { documentId });

			result.Content = JsonConvert.DeserializeObject(response.Resource);
			result.RequestInfo.RequestCharge = response.RequestCharge;

			return result;
		}

		#endregion
	}
}
