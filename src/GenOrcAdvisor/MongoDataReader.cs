using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GenOrcAdvisor
{
    internal class MongoDataReader
    {
        private readonly IOptions<MongoDBSettings> _mongoDBSettings;
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDataReader(IOptions<MongoDBSettings> mongoDBSettings)
        {
            _mongoDBSettings = mongoDBSettings;

            _mongoClient = new MongoClient(_mongoDBSettings.Value.ConnectionString);
            _mongoDatabase = _mongoClient.GetDatabase(_mongoDBSettings.Value.DatabaseName);
        }

        public async Task<BsonDocument> GetInstrumentStateData()
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>(_mongoDBSettings.Value.InstrumentStateCollectionName);

            var InstrumentStateData = (await collection.FindAsync(_ => true)).ToListAsync<BsonDocument>().Result;

            if (InstrumentStateData.Count == 1)
                return InstrumentStateData[0];
            else if (InstrumentStateData.Count > 1)
                throw new InvalidDataException($"'{_mongoDBSettings.Value.InstrumentStateCollectionName}' collection has multiple documents. Expecting state data to be single json doc.");
            else
                throw new InvalidDataException($"'{_mongoDBSettings.Value.InstrumentStateCollectionName}' collection has no data available. Expecting state data to be single json doc.");
        }

        public async Task<BsonDocument> GetNextDocument()
        {
            var collection = _mongoDatabase.GetCollection<BsonDocument>(_mongoDBSettings.Value.OrderMessagesCollectionName);

            var OrderMessagesData = (await collection.FindAsync(_ => true)).ToListAsync<BsonDocument>().Result;

            if (OrderMessagesData.Count > 1)
                return OrderMessagesData[0];
            else
                return new BsonDocument();
        }
        
        #region CRUD Operations Sample

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
        #endregion

    }
}
