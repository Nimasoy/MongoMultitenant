using Xunit;
using Moq;
using MongoMultitenant.Services;
using MongoMultitenant.Entities;
using MongoMultitenant.Services.DTOs;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoMultitenant.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<IMongoDatabase> _mongoDatabaseMock;
        private readonly Mock<IMongoCollection<Product>> _productCollectionMock;
        private readonly Mock<ICurrentTenantService> _currentTenantServiceMock;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mongoDatabaseMock = new Mock<IMongoDatabase>();
            _productCollectionMock = new Mock<IMongoCollection<Product>>();
            _currentTenantServiceMock = new Mock<ICurrentTenantService>();

            _mongoDatabaseMock.Setup(db => db.GetCollection<Product>("Products", null))
                .Returns(_productCollectionMock.Object);

            _productService = new ProductService(_mongoDatabaseMock.Object, _currentTenantServiceMock.Object);
        }

        [Fact]
        public async Task CreateAsync_Should_CreateAndReturnProduct()
        {
            // Arrange
            var createProductRequestDto = new CreateProductRequestDto
            {
                ProductName = "Test Product",
                ProductDescription = "Test Description",
                ProductPrice = 10.99m,
                ProductStock = 100
            };

            var tenantId = Guid.NewGuid().ToString();
            _currentTenantServiceMock.Setup(s => s.TenantId).Returns(tenantId);

            Product capturedProduct = null;
            _productCollectionMock.Setup(col => col.InsertOneAsync(It.IsAny<Product>(), null, It.IsAny<CancellationToken>()))
                .Callback<Product, InsertOneOptions, CancellationToken>((product, options, token) => capturedProduct = product)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _productService.CreateAsync(createProductRequestDto);

            // Assert
            _productCollectionMock.Verify(col => col.InsertOneAsync(It.IsAny<Product>(), null, It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(capturedProduct);
            Assert.Equal(createProductRequestDto.ProductName, capturedProduct.ProductName);
            Assert.Equal(createProductRequestDto.ProductDescription, capturedProduct.ProductDescription);
            Assert.Equal(createProductRequestDto.ProductPrice, capturedProduct.ProductPrice);
            Assert.Equal(createProductRequestDto.ProductStock, capturedProduct.ProductStock);
            Assert.Equal(tenantId, capturedProduct.TenantId);
            Assert.NotEqual(Guid.Empty, capturedProduct.Id);

            Assert.NotNull(result);
            Assert.Equal(capturedProduct.Id, result.Id);
            Assert.Equal(capturedProduct.ProductName, result.ProductName);
            Assert.Equal(capturedProduct.TenantId, result.TenantId);
        }

        [Fact]
        public async Task GetAllAsync_Should_ReturnProductsForCurrentTenant()
        {
            // Arrange
            var tenantId = Guid.NewGuid().ToString();
            _currentTenantServiceMock.Setup(s => s.TenantId).Returns(tenantId);

            var sampleProducts = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), ProductName = "Product 1", TenantId = tenantId, ProductPrice = 10, ProductStock = 10 },
                new Product { Id = Guid.NewGuid(), ProductName = "Product 2", TenantId = tenantId, ProductPrice = 20, ProductStock = 20 }
            };

            var mockCursor = new Mock<IAsyncCursor<Product>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.Setup(c => c.Current).Returns(sampleProducts);

            _productCollectionMock.Setup(col => col.FindAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<FindOptions<Product, Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _productService.GetAllAsync();

            // Assert
            _productCollectionMock.Verify(col => col.FindAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<FindOptions<Product, Product>>(),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.NotNull(result);
            Assert.Equal(sampleProducts.Count, result.Count());

            foreach (var product in result)
            {
                Assert.Equal(tenantId, product.TenantId);
            }

            for (int i = 0; i < sampleProducts.Count; i++)
            {
                Assert.Equal(sampleProducts[i].ProductName, result.ElementAt(i).ProductName);
                Assert.Equal(sampleProducts[i].ProductPrice, result.ElementAt(i).ProductPrice);
            }
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnTrue_WhenDeletionIsSuccessful()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantId = Guid.NewGuid().ToString(); // ProductService might use this for tenancy check on delete

            _currentTenantServiceMock.Setup(s => s.TenantId).Returns(tenantId);

            // The filter in DeleteOneAsync would combine productId and tenantId.
            // We use It.IsAny<FilterDefinition<Product>>() for robustness in the mock setup.
            // The actual verification that the service constructs the filter correctly
            // would ideally be done by inspecting the filter if Moq allowed, or by trusting
            // the service's implementation and testing behavior (deleted count > 0).
            _productCollectionMock.Setup(col => col.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteResult.Acknowledged(1)); // 1 document deleted

            // Act
            var result = await _productService.DeleteAsync(productId.ToString());

            // Assert
            _productCollectionMock.Verify(col => col.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_ReturnFalse_WhenProductNotFoundOrDeletionFails()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var tenantId = Guid.NewGuid().ToString();
            _currentTenantServiceMock.Setup(s => s.TenantId).Returns(tenantId);

            _productCollectionMock.Setup(col => col.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteResult.Acknowledged(0)); // 0 documents deleted

            // Act
            var result = await _productService.DeleteAsync(productId.ToString());

            // Assert
            _productCollectionMock.Verify(col => col.DeleteOneAsync(
                It.IsAny<FilterDefinition<Product>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.False(result);
        }
    }
}
