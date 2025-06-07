using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoMultitenant.Entities;
using MongoMultitenant.Services.DTOs;

namespace MongoMultitenant.Services
{
    public class ProductService : IProductService
    {
        private readonly IMongoCollection<Product> _products;
        private readonly ICurrentTenantService _currentTenantService;

        public ProductService(IMongoDatabase database, ICurrentTenantService currentTenantService)
        {
            _products = database.GetCollection<Product>("Products");
            _currentTenantService = currentTenantService;
        }

        public async Task<Product> CreateAsync(CreateProductRequestDto request)
        {
            var product = new Product
            {
                ProductName = request.Name,
                ProductDescription = request.Description,
                ProductPrice = request.Price,
                ProductStock = request.Stock,
                TenantId = _currentTenantService.TenantId
            };
            await _products.InsertOneAsync(product);
            return product;
        }
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _products.DeleteOneAsync(p => p.Id == id.ToString());
            return result.DeletedCount > 0;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var tenantId = _currentTenantService.TenantId;
            return await _products.Find(p => p.TenantId == tenantId).ToListAsync();
        }
    }
}
