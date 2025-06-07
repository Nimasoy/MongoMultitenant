using MongoMultitenant.Entities;
using MongoDB.Driver;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Runtime.CompilerServices;

namespace MongoMultitenant.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly IMongoCollection<Tenant> _tenantsCollection;
        public string? TenantId { get; set; }
        public CurrentTenantService(IMongoDatabase database)
        {
            _tenantsCollection = database.GetCollection<Tenant>("Tenants");
        }
        
        public async Task<bool> CheckTenantAsync(string tenantId)
        {
            var exits = await _tenantsCollection.Find(t => t.Id == tenantId).AnyAsync();
            if (exits)
            {
                TenantId = tenantId;
                return true;
            }
            return false;
        }   
    }
}
