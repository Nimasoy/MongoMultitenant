using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoMultitenant.Entities
{
    public class Tenant
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
