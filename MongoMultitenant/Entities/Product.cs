using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoMultitenant.Entities
{
    public class Product : BaseTenant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public int ProductPrice { get; set; }
        public int ProductStock { get; set; }
    }
}
