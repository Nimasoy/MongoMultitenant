using MongoMultitenant.Entities;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Runtime.CompilerServices;

namespace MongoMultitenant.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IMongoDatabase _database;
        public required string TenantId { get; set; }
        public CurrentTenantService(IMongoDatabase database)
        {
            _database = database;
        }
        
        public async Task<bool> CheckTenantAsync(string tenantId)
        {
            var tenantsCollection = _database.GetCollection<Tenant>("Tenants");
            var exits = await tenantsCollection.Find(t => t.Id == tenantId).AnyAsync();
            if (exits)
            {
                TenantId = tenantId;
                return true;
            }
            return false;
        }   
    }
}
