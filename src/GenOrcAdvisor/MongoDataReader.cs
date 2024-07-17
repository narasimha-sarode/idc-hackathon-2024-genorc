using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GenOrcAdvisor
{
    internal class MongoDataReader
    {
        private readonly string _mongoDbConString = "mongodb+srv://genorcmongodbadmin:hackathon%402024@genorc-mongodb.mongocluster.cosmos.azure.com/?authMechanism=SCRAM-SHA-256&retrywrites=false&maxIdleTimeMS=120000";
        private readonly string _databaseName = "testdb";

        public MongoDataReader()
        {
        }

        public MongoDataReader(IConfiguration configuration)
        {
            //if(configuration is null)
            //    throw new ArgumentNullException(nameof(configuration));
            //if(configuration["MongoDB:ConnectionString"] is null)
            //    throw new ArgumentNullException(nameof(configuration));

            //_mongoDbConString = configuration["MongoDB:ConnectionString"];
        }
        public List<BsonDocument> GetInstrumentStateData()
        {
            var client = new MongoClient(_mongoDbConString);

            var database = client.GetDatabase(_databaseName);

            var collection = database.GetCollection<BsonDocument>("CurrentStateofInstrument");

            return ReadDocuments(collection);

        }

        // Create
        static void CreateDocument(IMongoCollection<BsonDocument> collection)
        {
            var document = new BsonDocument
            {
                { "name", "John Doe" },
                { "age", 30 },
                { "profession", "Software Developer" }
            };

            collection.InsertOne(document);
            Console.WriteLine("Document inserted successfully!");
        }

        // Read
        static List<BsonDocument> ReadDocuments(IMongoCollection<BsonDocument> collection)
        {
            var list = collection.Find(_ => true).ToList();

            var abc = collection.FindSync(_ => true);
            return collection.Find(new BsonDocument()).ToList();
        }

        // Update
        static void UpdateDocument(IMongoCollection<BsonDocument> collection)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("name", "John Doe");
            var update = Builders<BsonDocument>.Update.Set("age", 31);

            var result = collection.UpdateOne(filter, update);

            if (result.ModifiedCount > 0)
            {
                Console.WriteLine("Document updated successfully!");
            }
            else
            {
                Console.WriteLine("No documents matched the filter criteria.");
            }
        }

        // Delete
        static void DeleteDocument(IMongoCollection<BsonDocument> collection)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("name", "John Doe");

            var result = collection.DeleteOne(filter);

            if (result.DeletedCount > 0)
            {
                Console.WriteLine("Document deleted successfully!");
            }
            else
            {
                Console.WriteLine("No documents matched the filter criteria.");
            }
        }

    }
}
