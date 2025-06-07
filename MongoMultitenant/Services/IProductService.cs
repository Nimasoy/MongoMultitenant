using MongoMultitenant.Entities;
using MongoMultitenant.Services.DTOs;

namespace MongoMultitenant.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product> CreateAsync(CreateProductRequestDto request);
        Task<bool> DeleteAsync(string id);
    }
}