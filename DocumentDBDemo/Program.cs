﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Using the db requires these imports
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;

namespace DocumentDBDemo {
    class MainClass {

        // Should get your own endpoint and auth key, but this is an example
		private const string EndpointUrl = "https://azuredocdbdemo.documents.azure.com:443/";
		private const string AuthorizationKey =
		   "BBhjI0gxdVPdDbS4diTjdloJq7Fp4L5RO/StTt6UtEufDM78qM2CtBZWbyVwFPSJIm8AcfDu2O+AfV T+TYUnBQ==";
        private static Database database;

		static void Main(string[] args) {
			try {
				CreateDocumentClient().Wait();
			}
			catch (Exception e) {
				Exception baseException = e.GetBaseException();
				Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
			}
			Console.ReadKey();
		}

		private static async Task CreateDocumentClient() {
			// Create a new instance of the DocumentClient
			using (var client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey)) {
				database = client.CreateDatabaseQuery("SELECT * FROM c WHERE c.id = 'myfirstdb'").AsEnumerable().First();
	
				await CreateCollection(client, "MyCollection1");
				await CreateCollection(client, "MyCollection2", "S2");

				//await CreateDatabase(client);
				//GetDatabases(client);
				//await DeleteDatabase(client);
				//GetDatabases(client);
			}
		}

        /******************************************
         * 
         * 
         * Creating the database using a .NET SDK 
         *
         *
         ******************************************/

		// Create the new database by creating a new database object. 
        // To create a new database, we only need to assign the Id property, 
        // which we are setting to “mynewdb” in a CreateDatabase task
		private async static Task CreateDatabase(DocumentClient client) {
			Console.WriteLine();
			Console.WriteLine("******** Create Database *******");

			var databaseDefinition = new Database { Id = "mynewdb" };
			var result = await client.CreateDatabaseAsync(databaseDefinition);
			var database = result.Resource;

			Console.WriteLine(" Database Id: {0}; Rid: {1}", database.Id, database.ResourceId);
			Console.WriteLine("******** Database Created *******");
		}

		/******************************************
         * 
         * 
         * Listing the databases
         *
         *
         ******************************************/

		private static void GetDatabases(DocumentClient client) {
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("******** Get Databases List ********");

			var databases = client.CreateDatabaseQuery().ToList();

			foreach (var database in databases) {
				Console.WriteLine(" Database Id: {0}; Rid: {1}", database.Id, database.ResourceId);
			}

			Console.WriteLine();
			Console.WriteLine("Total databases: {0}", databases.Count);
		}

		/******************************************
         * 
         * 
         * Deleting a database
         *
         *
         ******************************************/

		/*
         * 
         * This time, we can call AsEnumerable instead of ToList() because we don't actually need a 
         * list object. Expecting only result, calling AsEnumerable is sufficient so that we can get 
         * the first database object returned by the query with First(). This is the database object for 
         * tempdb1 and it has a SelfLink that we can use to call DeleteDatabaseAsync which deletes the database.
         * 
         */

		private async static Task DeleteDatabase(DocumentClient client) {
			Console.WriteLine("******** Delete Database ********");
			Database database = client
			   .CreateDatabaseQuery("SELECT * FROM c WHERE c.id = 'tempdb1'")
			   .AsEnumerable()
			   .First();
			await client.DeleteDatabaseAsync(database.SelfLink);
		}

		/******************************************
         * 
         * 
         * Creating a collection
         *
         *
         ******************************************/

		private async static Task CreateCollection(DocumentClient client, string collectionId,
   string offerType = "S1") {

			Console.WriteLine();
			Console.WriteLine("**** Create Collection {0} in {1} ****", collectionId, database.Id);

			var collectionDefinition = new DocumentCollection { Id = collectionId };
			var options = new RequestOptions { OfferType = offerType };
			var result = await client.CreateDocumentCollectionAsync(database.SelfLink,
			   collectionDefinition, options);
			var collection = result.Resource;

			Console.WriteLine("Created new collection");
			ViewCollection(collection);
		}

		private static void ViewCollection(DocumentCollection collection) {
			Console.WriteLine("Collection ID: {0} ", collection.Id);
			Console.WriteLine("Resource ID: {0} ", collection.ResourceId);
			Console.WriteLine("Self Link: {0} ", collection.SelfLink);
			Console.WriteLine("Documents Link: {0} ", collection.DocumentsLink);
			Console.WriteLine("UDFs Link: {0} ", collection.UserDefinedFunctionsLink);
			Console.WriteLine(" StoredProcs Link: {0} ", collection.StoredProceduresLink);
			Console.WriteLine("Triggers Link: {0} ", collection.TriggersLink);
			Console.WriteLine("Timestamp: {0} ", collection.Timestamp);
		}
	}
}
