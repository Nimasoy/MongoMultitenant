using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoMultitenant.Services;
using MongoMultitenant.Services.DTOs;
using ZstdSharp.Unsafe;

namespace MongoMultitenant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductRequestDto request)
        {
            var products = await _productService.CreateAsync(request);
            return Ok(products);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            var result = await _productService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
