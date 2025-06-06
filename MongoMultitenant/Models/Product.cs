using MongoDB.Bson;

namespace MongoMultitenant.Models
{
    public class Product
    {
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        public string ?ProductName { get; set; }
        public string ?Description { get; set; }
    }
}
